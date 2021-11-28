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
        private JumpService _jumpService;

        public UnitsController(UserService userService, SectorService sectorService, IClock clock, JumpService jumpService)
        {
            _userService = userService;
            _sectorService = sectorService;
            _clock = clock;
            _jumpService = jumpService;
        }

        // GET /Users/{userId}/Units
        [HttpGet("{userId}/Units")]
        public ActionResult<List<GetUnitDto>> Get(string userId)
        {
            List<Unit> units = _userService.Units[userId];
            return units.Select(Unit => new GetUnitDto(Unit)).ToList();
        }

        // GET /Users/{userId}/Units/{unitId}
        [HttpGet("{userId}/Units/{unitId}")]
        public async Task<ActionResult<GetUnitDto>> Get(string userId, string unitId)
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
                        return new GetUnitDto(unit);
                    }
                    else
                    {   // Contient l’arrivée au lieu
                        if(timeBeforeArrival > 0 && timeBeforeArrival <= 2)
                        {
                            int delay = Convert.ToInt32((arrival - _clock.Now).TotalMilliseconds);
                            await _clock.Delay(delay);
                            return new GetUnitDto(_userService.Units[userId].Find(Unit => Unit.Id == unitId));
                        }
                        else
                        {   // 0 secondes: 200 OK
                            return new GetUnitDto(unit);
                        }
                        
                    }
                }
                else return new GetUnitDto(unit);
            }  
        }

        // PUT /Users/{userId}/Units/{unitId}
        [HttpPut("{userId}/Units/{unitId}")]
        public async Task<ActionResult<GetUnitDto>> Put(string userId, string unitId, CreateUnitDto spaceship)
        {

            if (unitId != spaceship.Id)
            {
                return BadRequest();
            }

            StarSystem destinationSystem = _sectorService.Systems.Find(StarSystem => StarSystem.Name == spaceship.DestinationSystem);
            Planet destinationPlanet = _sectorService.GetAllPlanets().Find(Planet => Planet.Name == spaceship.DestinationPlanet);

            List<Unit> units;
            Unit unt;
            try
            {
                units = _userService.Units[userId];
                unt = units.Find(Unit => Unit.Id == unitId);
            }
            catch (KeyNotFoundException)
            {
                _userService.Units.Add(userId, new List<Unit> { });
                unt = null;
            }
            

            // Unit does not exists
            if (unt == null)
            {
                Unit unit = spaceship.ToUnit();

                // Admin or Shard
                if (HttpContext.User.IsInRole(Constants.Roles.Admin) || HttpContext.User.IsInRole(Constants.Roles.Shard))
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

                return new GetUnitDto(unit);
            }
            else
            {
                Unit oldUnit = _userService.Units[userId].Find(Unit => Unit.Id == unitId);
                Unit newUnit = spaceship.ToUnit();

                // Jump
                if(spaceship.DestinationShard != null)
                {
                    if (HttpContext.User.IsInRole(Constants.Roles.User) || HttpContext.User.IsInRole(Constants.Roles.Admin))
                    {
                        /*
                        try
                        {
                            await _jumpService.SystemAndPlanetExists(spaceship.DestinationSystem, spaceship.DestinationPlanet, spaceship.DestinationShard);
                        }
                        catch (Exception ex)
                        {
                            return BadRequest($"{ex.Message}");
                        }
                        */
                        User user = _userService.Users.Find(User => User.Id == userId);
                        var unitUri = await _jumpService.Jump(newUnit, user, spaceship.DestinationShard);
                        _userService.Units[userId].Remove(oldUnit);
                        return RedirectPermanentPreserveMethod(unitUri);
                    }
                    else
                    {
                        return Forbid("You're not authorize to redirect shards");
                    }
                }

                // Cargo
                if (spaceship.ResourcesQuantity != null)
                {
                    if(spaceship.Type != "cargo")
                    {
                        return BadRequest("Can only load / unload resources on Cargo spaceship");
                    }
                    Building buildingOnCurrentPlanet = _userService.Buildings[userId]
                        .Find(building => building.Planet == spaceship.Planet && building.GetType() == typeof(Starport));
                    // Check if there's a Starport on the planet
                    if(buildingOnCurrentPlanet != null)
                    {
                        User user = _userService.Users.Find(User => User.Id == userId);
                        Dictionary<ResourceKind, int> resourcesToLoadUnload = (oldUnit as Cargo)
                            .ResourcesToLoadUnload(spaceship.ResourcesQuantity.ToDictionary(
                                resource => (ResourceKind)Enum.Parse(typeof(ResourceKind), Utils.Strings.Capitalize(resource.Key)), 
                                resource => resource.Value
                            ));
                        foreach(var resource in resourcesToLoadUnload)
                        {
                            if(resource.Value > 0) // load
                            {
                                if(user.ResourcesQuantity[resource.Key] - resource.Value > 0)
                                {
                                    user.ResourcesQuantity[resource.Key] = user.ResourcesQuantity[resource.Key] - resource.Value;
                                }
                                else return BadRequest($"User doesn't have enough resources for {resource.Key}. To add/deduce: {resource.Value}, current {user.ResourcesQuantity[resource.Key]}");
                            }
                            else // unload
                            {
                                user.ResourcesQuantity[resource.Key] += -resource.Value; // - to convert to positive int
                            }
                        }
                    }
                    else
                    {
                        return BadRequest("Could not find Starport on the current planet");
                    }
                }

                if (destinationSystem == null)
                {
                    return NotFound("Destination System not found");
                }
                
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
                return new GetUnitDto(newUnit);
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

