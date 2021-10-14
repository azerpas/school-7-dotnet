using Microsoft.AspNetCore.Mvc;
using Shard.Uni.Models;
using Shard.Uni.Services;

namespace Shard.Uni.Controllers
{
    [ApiController]
    [Route("users")]
    public class BuildingsController : ControllerBase
    {

        private UserService _userService;
        private readonly SectorService _sectorService;

        public BuildingsController(UserService userService, SectorService sectorService)
        {
            _userService = userService;
            _sectorService = sectorService;
        }

        // POST /users/{userId}/Buildings
        [HttpPost("/users/{userId}/Buildings")]
        public ActionResult<Building> CreateBuilding(string userId, [FromBody] CreateBuilding createBuilding)
        {
            User user = _userService.Users.Find(user => user.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            Unit unit = _userService.Units[user.Id].Find(Unit => Unit.Id == createBuilding.BuilderId && Unit.Type == "builder");

            if (unit == null)
            {
                return NotFound("Unit not found");
            }

            Building building = new Building(createBuilding.Id, "mine", unit.System, unit.Planet);
            _userService.Buildings[user.Id].Add(building);

            return building;
        }
    }
}

