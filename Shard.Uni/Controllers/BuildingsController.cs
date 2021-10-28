using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;
using Shard.Uni.Models;
using Shard.Uni.Services;
using System;

namespace Shard.Uni.Controllers
{
    [ApiController]
    [Route("users")]
    public class BuildingsController : ControllerBase
    {

        private UserService _userService;
        private readonly SectorService _sectorService;
        private IClock _clock;

        public BuildingsController(UserService userService, SectorService sectorService, IClock clock)
        {
            _userService = userService;
            _sectorService = sectorService;
            _clock = clock;
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
            if (!Building.GetBuildingTypes().Contains(createBuilding.Type))
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

            Planet planet = _sectorService.Systems
                .Find(System => unit.System == System.Name).Planets
                .Find(Planet => unit.Planet == Planet.Name);

            Building building = new Building(createBuilding.Id, "mine", unit.System, unit.Planet, createBuilding.ResourceCategory, _clock);
            _userService.Buildings[user.Id].Add(building);

            // Built in 5 minutes
            _clock.CreateTimer(
                _ => building.IsBuilt = true,
                null,
                new TimeSpan(0, 5, 0),
                new TimeSpan(0)
            );

            // Then every minute
            //      - +1 resource to owner
            //      - -1 resource from planet
            _clock.CreateTimer(
                _ =>
                {
                    // GetResourcesByType
                },
                null,
                new TimeSpan(0,5,0),
                new TimeSpan(0,1,0)
            );

            return building;
        }
    }
}

