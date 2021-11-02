using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Shard.Shared.Web.IntegrationTests
{
    public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
    {
        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task BuildingStarportThenFetchingAllBuildingsIncludesStarport()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            var response = await client.GetAsync($"{userPath}/buildings");
            await response.AssertSuccessStatusCode();

            var buildings = (await response.Content.ReadAsAsync<JArray>()).ToArray();
            Assert.Single(buildings);
            var building = buildings[0].Value<JObject>();

            Assert.Equal(originalBuilding.ToString(), building.ToString());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task BuildingStarportThenFetchingBuildingByIdReturnsStarport()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);
            var building = await RefreshBuilding(client, userPath, originalBuilding);

            Assert.Equal(originalBuilding.ToString(), building.ToString());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task BuildingStarportThenWaiting4MinReturnsUnbuiltStarport()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(4));
            var building = await RefreshBuilding(client, userPath, originalBuilding);

            Assert.False(building["isBuilt"].Value<bool>());
            Assert.Equal(fakeClock.Now.AddMinutes(1), building["estimatedBuildTime"].Value<DateTime>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task BuildingStarportThenWaiting5MinReturnsBuiltStarport()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));
            var building = await RefreshBuilding(client, userPath, originalBuilding);

            Assert.True(building["isBuilt"].Value<bool>());
            Assert.True(!building.ContainsKey("estimatedBuildTime")
                || building["estimatedBuildTime"].Type == JTokenType.Null);
        }
    }
}
