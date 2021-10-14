using Microsoft.AspNetCore.Mvc;
using Shard.Uni.Services;
using Shard.Uni.Models;
using System.Collections.Generic;
using Shard.Shared.Core;
using System.Linq;

namespace Shard.Uni.Controllers
{
    [ApiController]
    [Route("users")]
    public class UnitsController : ControllerBase
    {

        private UserService _userService;
        private readonly SectorService _sectorService;

        public UnitsController(UserService userService, SectorService sectorService)
        {
            _userService = userService;
            _sectorService = sectorService;
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
        public ActionResult<Unit> Get(string userId, string unitId)
        {
            List<Unit> units = _userService.Units[userId];
            Unit unit = units.Find(Unit => Unit.Id == unitId);
            if(unit == null)
            {
                return NotFound();
            }
            else
            {
                return unit;
            }  
        }

        // PUT /Users/{userId}/Units/{unitId}
        [HttpPut("{userId}/Units/{unitId}")]
        public ActionResult<Unit> Put(string userId, string unitId, Unit spaceship)
        {
            List<Unit> units = _userService.Units[userId];

            if (unitId != spaceship.Id)
            {
                return BadRequest();
            }

            if (!Unit.getAuthorizedTypes().Contains(spaceship.Type))
            {
                return BadRequest("Unrecognized type of Unit");
            }

            StarSystem destinationSystem = _sectorService.Systems.Find(StarSystem => StarSystem.Name == spaceship.DestinationSystem);
            Planet destinationPlanet = _sectorService.GetAllPlanets().Find(Planet => Planet.Name == spaceship.DestinationPlanet);
            bool destinationPlanetIsInDestinationSystem = destinationSystem.Planets.Exists(Planet => Planet.Name == destinationPlanet.Name);

            if (destinationSystem == null)
            {
                return NotFound("Destination System not found");
            }

            if (destinationPlanet == null)
            {
                return NotFound("Destination Planet not found");
            }

            if (destinationPlanetIsInDestinationSystem == false)
            {
                return BadRequest("Destination planet is not in destination system");
            }

            Unit unt = units.Find(Unit => Unit.Id == unitId);

            if (unt == null)
            {
                _userService.Units[userId].Add(spaceship);
            }
            else
            {
                Unit oldUnit = _userService.Units[userId].Find(Unit => Unit.Id == unitId);
                _userService.Units[userId].Remove(oldUnit);
                _userService.Units[userId].Add(spaceship);
                
                bool sameSystem = spaceship.DestinationSystem == spaceship.System;
                spaceship.moveTo(system: destinationSystem.Name, planet: destinationPlanet.Name);
            }
            return spaceship;
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
                return unitLocation;
            }
        }
    }
}

