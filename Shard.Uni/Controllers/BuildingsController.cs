using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;
using Shard.Uni.Models;
using Shard.Uni.Repositories;
using Shard.Uni.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private IBuildingRepository _repository; 

        public BuildingsController(UserService userService, SectorService sectorService, IClock clock, IBuildingRepository buildingRepository)
        {
            _userService = userService;
            _sectorService = sectorService;
            _clock = clock;
            _repository = buildingRepository;
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

            if (!Building.GetBuildingTypes().Contains(createBuilding.Type)) return BadRequest("Wrong Building Type");

            Unit unit = _userService.Units[user.Id].Find(Unit => Unit.Id == createBuilding.BuilderId);

            // FIXME: test BuildingWithIncorrectBuilderIdSend400 says we need to return 400?
            // Should be 404
            if (unit == null) return BadRequest("Unit not found");

            if (unit.GetType() != typeof(Builder)) return BadRequest("Unit is not a builder");

            if (unit.Planet == null) return BadRequest("Unit is not over a planet");

            Planet planet = _sectorService.Systems
                .Find(System => unit.System == System.Name).Planets
                .Find(Planet => unit.Planet == Planet.Name);

            Building building = createBuilding.ToClass(unit.System, unit.Planet, _clock);
            _userService.Buildings[user.Id].Add(building);

            // Built in 5 minutes
            building.StartConstruction(_clock, planet, user, createBuilding.ResourceCategory);

            return building;
        }

        // GET /users/{userId}/Buildings
        [HttpGet("/users/{userId}/Buildings")]
        public ActionResult<List<BuildingDto>> Get(string userId)
        {
            User user = _userService.Users.Find(user => user.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }
            // TODO: missing resourceCategory and queue for mine and starport
            return _userService.Buildings[user.Id].Select(building => new BuildingDto(building)).ToList();
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
                        if (timeBeforeBuildingReady >= 0 && timeBeforeBuildingReady <= 2)
                        {
                            building = _userService.Buildings[userId].Find(Building => Building.Id == buildingId);
                            try
                            {
                                await building.Construction;
                            }
                            catch(TaskCanceledException ex)
                            {
                                return NotFound();
                            }
                            
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

        // POST /users/{userId}/Buildings
        [HttpPost("/users/{userId}/Buildings/{starportId}/queue")]
        public ActionResult<Unit> AddToQueue(string userId, string starportId, [FromBody] QueueUnit addUnit)
        {
            User user = _userService.Users.Find(user => user.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            List<Building> buildings = _userService.Buildings[user.Id];
            Building starport = buildings.Find(Building => Building.Id == starportId);

            if (starport == null)
            {
                return NotFound("Starport not found");
            }

            if (starport.IsBuilt != true)
            {
                return BadRequest("Starport is not yet built");
            }

            if(starport.Type != "starport")
            {
                return BadRequest("Wrong building type");
            }

            try
            {
                (starport as Starport).AddToQueue(addUnit, user);
                Unit unit = addUnit.ToClass(starport.System, starport.Planet);
                _userService.Units[user.Id].Add(unit);
                return unit;
            }
            catch (NotEnoughResources ex)
            {
                return BadRequest("Not enough resources");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
