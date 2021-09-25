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

        public UnitsController(UserService userService)
        {
            _userService = userService;
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
                // TODO: resourcesQuantity { additionalProp1: ... } 
                return Ok(new { system = unit.System, planet = unit.Planet });
            }
        }
    }
}

