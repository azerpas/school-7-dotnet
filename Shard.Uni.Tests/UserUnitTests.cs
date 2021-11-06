using System;
using System.Collections.Generic;
using Shard.Uni.Models;
using Shard.Uni.Services;
using Xunit;
using Shard.Shared.Core;
using Shard.Shared.Web.IntegrationTests.Clock;
using System.Threading.Tasks;

namespace Shard.Uni.Tests
{
    public class UserUnitTests
    {

        UserService _userService;
        SectorService _sectorService;
        FakeClock _fakeClock;

        public UserUnitTests()
        {
            _userService = new UserService();
            _sectorService = new SectorService(new MapGenerator(new MapGeneratorOptions { Seed = "Uni" }));
            _fakeClock = new FakeClock();
    }

        [Fact]
        public void NoUsersOnStartUp()
        {
            List<User> users = _userService.Users;
            Assert.Equal(users,new List<User>());
        }

        public User GetUser()
        {
            string uuid = Guid.NewGuid().ToString();
            User user = new User(uuid, "azerpas");
            _userService.Users.Add(user);
            return user;
        }

        [Fact]
        public void UserGenerated()
        {
            string uuid = Guid.NewGuid().ToString();
            User user = new User(uuid, "azerpas");
            _userService.Users.Add(user);
            Assert.Matches("[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12}", _userService.Users[0].Id);
            Assert.Equal("azerpas", _userService.Users[0].Pseudo);
            Assert.Contains(System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm"), _userService.Users[0].DateOfCreation);
        }

        [Fact]
        public void UnitGenerated()
        {
            string uuid = Guid.NewGuid().ToString();
            User user = new User(uuid, "azerpas");
            _userService.Users.Add(user);
            Random random = new Random();
            int index = random.Next(_sectorService.Systems.Count);
            StarSystem system = _sectorService.Systems[index];
            index = random.Next(system.Planets.Count);
            Planet planet = system.Planets[index];
            Unit unit = new Unit("scout", system.Name, planet.Name);
            _userService.Units.Add(user.Id, new List<Unit> { unit });
            Assert.Matches("[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12}", _userService.Units[user.Id][0].Id);
            Assert.Equal("scout", _userService.Units[user.Id][0].Type);
            Assert.Equal(planet.Name, _userService.Units[user.Id][0].Planet);
        }

        [Fact]
        public async Task MoveUnit()
        {
            Random random = new Random();
            int index = random.Next(_sectorService.Systems.Count);
            StarSystem system = _sectorService.Systems[index];
            index = random.Next(system.Planets.Count);
            Planet fromPlanet = system.Planets[index];
            index = random.Next(system.Planets.Count);
            Planet toPlanet = system.Planets[index];

            Unit unit = new Unit("scout", system.Name, fromPlanet.Name);
            unit.MoveTo(system.Name, toPlanet.Name, _fakeClock);

            await _fakeClock.Advance(new TimeSpan(0, 0, 15));
            Assert.Equal(unit.Planet, toPlanet.Name);
        }
    }
}
