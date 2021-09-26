using Microsoft.AspNetCore.Mvc;
using Shard.Uni.Services;
using Shard.Uni.Models;
using System.Collections.Generic;

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
        public ActionResult<Unit> Put(string userId, string unitId, Unit unit)
        {
            List<Unit> units = _userService.Units[userId];

            if (unitId != unit.Id)
            {
                return BadRequest();
            }

            Unit unt = units.Find(Unit => Unit.Id == unitId);

            if (unt == null)
            {
                _userService.Units[userId].Add(unit);
            }
            else
            {
                Unit oldUnit = _userService.Units[userId].Find(Unit => Unit.Id == unitId);
                _userService.Units[userId].Remove(oldUnit);
                _userService.Units[userId].Add(unit);
            }
            return unit;
        }

        // GET /Users/{userId}/Units/{unitId}/locations
        [HttpGet("{userId}/Units/{unitId}/locations")]
        public ActionResult<Unit> GetLocations(string userId, string unitId)
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
                return Ok(new {
                    system = unit.System,
                    planet = unit.Planet,
                    resourcesQuantity = planet.ResourceQuantity
                });
            }
        }
    }
}

