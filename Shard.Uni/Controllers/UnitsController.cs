using Microsoft.AspNetCore.Mvc;
using Shard.Uni.Services;
using Shard.Uni.Models;
using System.Collections.Generic;
using Shard.Shared.Core;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace Shard.Uni.Controllers
{
    [ApiController]
    [Route("users")]
    public class UnitsController : ControllerBase
    {

        private UserService _userService;
        private readonly SectorService _sectorService;
        private IClock _clock;

        public UnitsController(UserService userService, SectorService sectorService, IClock clock)
        {
            _userService = userService;
            _sectorService = sectorService;
            _clock = clock;
        }

        // GET /Users/{userId}/Units
        [HttpGet("{userId}/Units")]
        public ActionResult<List<Unit>> Get(string userId)
        {
            List<Unit> units = _userService.Units[userId];
            return units;
        }

        // GET /Users/{userId}/Units/{unitId}
        [HttpGet("{userId}/Units/{unitId}")]
        public async Task<ActionResult<Unit>> Get(string userId, string unitId)
        {
            List<Unit> units = _userService.Units[userId];
            Unit unit = units.Find(Unit => Unit.Id == unitId);

            if(unit == null)
            {
                return NotFound();
            }
            else
            {
                if(unit.GetType().IsSubclassOf(typeof(FightingUnit)))
                {
                    FightingUnit fightingUnit = unit as FightingUnit;
                    if (fightingUnit.Health <= 0)
                    {
                        _userService.Units[userId].Remove(unit);
                        return NotFound();
                    }
                }

                if (unit.EstimatedTimeOfArrival != null)
                {
                    DateTime arrival = DateTime.Parse(unit.EstimatedTimeOfArrival);
                    DateTime now = _clock.Now;
                    double timeBeforeArrival = (arrival - now).TotalSeconds;
                    if (timeBeforeArrival > 2.0)
                    {   // Contient la destination
                        unit.Planet = null;
                        return unit;
                    }
                    else
                    {   // Contient l’arrivée au lieu
                        if(timeBeforeArrival > 0 && timeBeforeArrival <= 2)
                        {
                            int delay = Convert.ToInt32((arrival - _clock.Now).TotalMilliseconds);
                            await _clock.Delay(delay);
                            return _userService.Units[userId].Find(Unit => Unit.Id == unitId);
                        }
                        else
                        {   // 0 secondes: 200 OK
                            return unit;
                        }
                        
                    }
                }
                else return unit;
            }  
        }

        // PUT /Users/{userId}/Units/{unitId}
        [HttpPut("{userId}/Units/{unitId}")]
        public ActionResult<Unit> Put(string userId, string unitId, CreateUnitDto spaceship)
        {
            List<Unit> units = _userService.Units[userId];

            if (unitId != spaceship.Id)
            {
                return BadRequest();
            }

            StarSystem destinationSystem = _sectorService.Systems.Find(StarSystem => StarSystem.Name == spaceship.DestinationSystem);
            Planet destinationPlanet = _sectorService.GetAllPlanets().Find(Planet => Planet.Name == spaceship.DestinationPlanet);

            Unit unt = units.Find(Unit => Unit.Id == unitId);

            // Unit does not exists
            if (unt == null)
            {
                Unit unit = spaceship.ToUnit();

                // Admin
                if (HttpContext.User.IsInRole(Constants.Roles.Admin))
                {
                    _userService.Units[userId].Add(unit);
                } 
                // User
                else if (HttpContext.User.IsInRole(Constants.Roles.User))
                {
                    return Forbid();
                }
                // Unauthenticated
                else
                {
                    return Unauthorized();
                }

                return unit;
            }
            else
            {
                if (destinationSystem == null)
                {
                    return NotFound("Destination System not found");
                }

                Unit oldUnit = _userService.Units[userId].Find(Unit => Unit.Id == unitId);
                Unit newUnit = spaceship.ToUnit();
                _userService.Units[userId].Remove(oldUnit);
                _userService.Units[userId].Add(newUnit);
                
                bool sameSystem = spaceship.DestinationSystem == spaceship.System;
                newUnit.MoveTo(system: destinationSystem.Name, planet: destinationPlanet?.Name, _clock);

                if(!sameSystem || destinationPlanet?.Name != spaceship.Planet)
                {
                    // Cancel current constructions
                    List<Building> buildingsBuilt = _userService.Buildings[userId].FindAll(
                        Building => Building.BuilderId == spaceship.Id
                    );
                    foreach (Building building in buildingsBuilt)
                    {
                        // Cancel any Task related to the building
                        building.TokenSource.Cancel();
                        // Remove the building from the list of buildings
                        _userService.Buildings[userId].RemoveAll(Building => Building.Id == building.Id);
                    }
                }
                return newUnit;
            }
        }

        // GET /Users/{userId}/Units/{unitId}/location
        [HttpGet("{userId}/Units/{unitId}/location")]
        public ActionResult<UnitLocationDetailDto> GetLocations(string userId, string unitId)
        {
            List<Unit> units = _userService.Units[userId];
            Unit unit = units.Find(Unit => Unit.Id == unitId);
            if (unit == null)
            {
                return NotFound();
            }
            else
            {
                Planet planet = _sectorService
                    .Systems.Find(System => System.Name == unit.System)
                    .Planets.Find(Planet => Planet.Name == unit.Planet);
                // Setting the Key to lower because of the test verification :
                // https://gitlab.com/efrei-p2023/efrei-p2023-csharp/-/blob/v2/Shard.Shared.Web.IntegrationTests/BaseIntegrationTests.ScoutTests.cs#L225-235
                UnitLocationDetailDto unitLocation = new UnitLocationDetailDto(unit.System, planet);
                if(unit.GetType() == typeof(Builder))
                {
                    unitLocation.resourcesQuantity = null;
                }
                return unitLocation;
            }
        }
    }
}

