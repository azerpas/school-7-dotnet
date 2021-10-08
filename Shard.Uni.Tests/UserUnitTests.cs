using System;
using System.Collections.Generic;
using Shard.Uni.Models;
using Shard.Uni.Services;
using Xunit;
using Shard.Shared.Core;

namespace Shard.Uni.Tests
{
    public class UserUnitTests
    {

        UserService _userService;
        SectorService _sectorService;

        public UserUnitTests()
        {
            _userService = new UserService();
            _sectorService = new SectorService(new MapGenerator(new MapGeneratorOptions { Seed = "Uni" }));
        }

        [Fact]
        public void NoUsersOnStartUp()
        {
            List<User> users = _userService.Users;
            Assert.Equal(users,new List<User>());
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

        [Fact(Skip = "No yet implemented")]
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
    }
}
