using System;
using System.Collections.Generic;
using Shard.Uni.Models;
using Shard.Uni.Services;
using Shard.Uni.Tests;
using Xunit;
using Shard.Shared.Core;
using Shard.Shared.Web.IntegrationTests.Clock;
using System.Threading.Tasks;

namespace Shard.Uni.Tests
{
    public class BuildingUnitTests
    {

        UserService _userService;
        SectorService _sectorService;
        FakeClock _fakeClock;
        UserUnitTests _userUnitTests;

        public BuildingUnitTests()
        {
            _userService = new UserService();
            _sectorService = new SectorService(new MapGenerator(new MapGeneratorOptions { Seed = "Uni" }));
            _fakeClock = new FakeClock();
            _userUnitTests = new UserUnitTests();
        }

        public Unit GetUnit(string type)
        {
            Random random = new Random();
            int index = random.Next(_sectorService.Systems.Count);
            StarSystem system = _sectorService.Systems[index];
            index = random.Next(system.Planets.Count);
            Planet planet = system.Planets[index];
            return UnitFromType(type, system.Name, planet.Name);
        }

        public Unit UnitFromType(string type, string system, string? planet)
        {
            string id = Guid.NewGuid().ToString();
            switch (type)
            {
                case "builder":
                    return new Builder(id, system, planet);
                case "scout":
                    return new Scout(id, system, planet);
                case "bomber":
                    return new Bomber(id, system, planet);
                case "fighter":
                    return new Fighter(id, system, planet);
                case "cruiser":
                    return new Cruiser(id, system, planet);
                default:
                    throw new UnrecognizedUnit("Unrecognized type of Unit");
            }
        }

        [Fact]
        public void CanStartAConstruction()
        {
            Unit unit = GetUnit("scout");
            User user = _userUnitTests.GetUser();
            Planet planet = _sectorService.GetAllPlanets().Find(Planet => Planet.Name == unit.Planet);
            Building building = new Building(Guid.NewGuid().ToString(), "mine", unit.System, unit.Planet, "solid", unit.Id, _fakeClock);
            building.StartConstruction(_fakeClock, planet, user, "solid");
            Assert.Equal(TaskStatus.WaitingForActivation, building.Construction.Status);
        }

        [Fact]
        public async void MineIsBuilt_After5minOfConstruction()
        {
            Unit unit = GetUnit("scout");
            User user = _userUnitTests.GetUser();
            Planet planet = _sectorService.GetAllPlanets().Find(Planet => Planet.Name == unit.Planet);
            Building building = new Building(Guid.NewGuid().ToString(), "mine", unit.System, unit.Planet, "solid", unit.Id, _fakeClock);
            building.StartConstruction(_fakeClock, planet, user, "solid");
            await _fakeClock.Advance(new TimeSpan(0, 5, 0));
            await building.Construction;
            Assert.True(building.IsBuilt);
        }

        [Fact(Skip = "Error on concurrence")]
        public async void MineResourcesOnAPlanet_DecreasePlanetResources()
        {
            Unit unit = GetUnit("scout");
            User user = _userUnitTests.GetUser();
            Planet planet = _sectorService.GetAllPlanets().Find(Planet => Planet.Name == unit.Planet);
            planet.ResourceQuantity[ResourceKind.Gold] = 45;
            planet.ResourceQuantity[ResourceKind.Iron] = 120;
            planet.ResourceQuantity[ResourceKind.Titanium] = 50;
            Building building = new Building(Guid.NewGuid().ToString(), "mine", unit.System, unit.Planet, "solid", unit.Id, _fakeClock);
            building.StartConstruction(_fakeClock, planet, user, "solid");
            await _fakeClock.Advance(new TimeSpan(0, 5, 0));
            await building.Construction;
            Assert.Equal(45, planet.ResourceQuantity[ResourceKind.Gold]);
            Assert.Equal(50, planet.ResourceQuantity[ResourceKind.Titanium]);
            Assert.Equal(120, planet.ResourceQuantity[ResourceKind.Iron]);
            await _fakeClock.Advance(new TimeSpan(0, 70, 1));
            Assert.Equal(50, planet.ResourceQuantity[ResourceKind.Iron]);
            await _fakeClock.Advance(new TimeSpan(0, 1, 1));
            Assert.Equal(49, planet.ResourceQuantity[ResourceKind.Titanium]);
        }
    }
}