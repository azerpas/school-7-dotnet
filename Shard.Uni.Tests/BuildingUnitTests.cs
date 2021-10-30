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
    public class BuildingUnitTests
    {

        UserService _userService;
        SectorService _sectorService;
        FakeClock _fakeClock;

        public BuildingUnitTests()
        {
            _userService = new UserService();
            _sectorService = new SectorService(new MapGenerator(new MapGeneratorOptions { Seed = "Uni" }));
            _fakeClock = new FakeClock();
        }
    }
}