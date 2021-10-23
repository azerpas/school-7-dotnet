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

            if(createBuilding.BuilderId == null)
            {
                return BadRequest("Please input a builder id");
            }

            // Test BuildingWithIncorrectBuildingTypeSends400
            if (!Building.getBuildingTypes().Contains(createBuilding.Type))
            {
                return BadRequest("Wrong Building Type");
            }

            Unit unit = _userService.Units[user.Id].Find(Unit => Unit.Id == createBuilding.BuilderId);

            if (unit == null)
            {
                // FIXME: test BuildingWithIncorrectBuilderIdSend400 says we need to return 400?
                // Should be 404
                return BadRequest("Unit not found");
            }

            // Test BuildingWithUnitNotOverPlanetSends404
            if (unit.Planet == null)
            {
                return BadRequest("Unit is not over a planet");
            }

            Building building = new Building(createBuilding.Id, "mine", unit.System, unit.Planet);
            _userService.Buildings[user.Id].Add(building);

            return building;
        }
    }
}

