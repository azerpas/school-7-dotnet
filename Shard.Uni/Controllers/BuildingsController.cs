using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;
using Shard.Uni.Models;
using Shard.Uni.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

            if (createBuilding.BuilderId == null)
            {
                return BadRequest("Please input a builder id");
            }

            // Fixing missing id in Test `CanBuildMineOnPlanet`
            // https://gitlab.com/efrei-p2023/efrei-p2023-csharp/-/issues/1
            if (createBuilding.Id == null)
            {
                createBuilding.Id = Guid.NewGuid().ToString();
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

            Building building = new Building(createBuilding.Id, "mine", unit.System, unit.Planet, createBuilding.ResourceCategory, createBuilding.BuilderId, _clock);
            _userService.Buildings[user.Id].Add(building);

            // Built in 5 minutes
            _clock.CreateTimer(
                _ => {
                    building.IsBuilt = true;
                    building.EstimatedBuildTime = null;
                    building.BuilderId = null;
                },
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
                    if(building == null)
                    { // in case we've cancelled the building construction by moving
                        return;
                    }
                    if (building.IsBuilt != true)
                    {
                        return;
                    }
                    ResourceKind resource = planet.GetResourceToMine(createBuilding.ResourceCategory);
                    try
                    {
                        planet.Mine(resource);
                    }
                    catch (NoResourcesAvailableException)
                    {
                        Console.WriteLine($"[WARNING] No more resources are available for resource {resource.ToString()}");
                        return;
                    }
                    user.ResourcesQuantity[resource] = user.ResourcesQuantity[resource] + 1;
                },
                null,
                new TimeSpan(0, 6, 0), // in 6 minutes (we start one minute after build has ended) ...
                new TimeSpan(0, 1, 0) // ... execute the code above every minute
            );

            return building;
        }

        // GET /users/{userId}/Buildings
        [HttpGet("/users/{userId}/Buildings")]
        public ActionResult<List<Building>> Get(string userId)
        {
            User user = _userService.Users.Find(user => user.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return _userService.Buildings[user.Id];
        }

        // GET /users/{userId}/Buildings/{buildingId}
        [HttpGet("/users/{userId}/Buildings/{buildingId}")]
        public async Task<ActionResult<Building>> Get(string userId, string buildingId)
        {
            User user = _userService.Users.Find(user => user.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            List<Building> buildings = _userService.Buildings[user.Id];

            Building building = buildings.Find(Building => Building.Id == buildingId);

            if (building == null)
            {
                return NotFound();
            }
            else
            {
                if (building.EstimatedBuildTime != null)
                {
                    DateTime finishedAt = DateTime.Parse(building.EstimatedBuildTime);
                    DateTime now = _clock.Now;

                    double timeBeforeBuildingReady = (finishedAt - now).TotalSeconds;
                    if (timeBeforeBuildingReady > 2.0)
                    {
                        return building;
                    }
                    else
                    {
                        if (timeBeforeBuildingReady > 0 && timeBeforeBuildingReady <= 2)
                        {
                            int delay = Convert.ToInt32((finishedAt - _clock.Now).TotalMilliseconds);
                            await _clock.Delay(delay);
                            building = _userService.Buildings[userId].Find(Building => Building.Id == buildingId);
                            if(building == null)
                            { // If building has been moved during the 2sec delay
                                return NotFound();
                            }
                            else return building;
                        }
                        else
                        {
                            return building;
                        }

                    }
                }
                else return building;
            }
        }
    }
}
