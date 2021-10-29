using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutOnBuiltStarportImmediatlyReturnsOne()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));
            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertSuccessStatusCode();

            var unit = await response.Content.ReadAsAsync<JObject>();
            Assert.NotNull(unit["id"].Value<string>());
            Assert.Equal("scout", unit["type"].Value<string>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutOnBuiltStarportCost5Carbon5Iron()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));

            await AssertResourceQuantity(client, userPath, "carbon", 20);
            await AssertResourceQuantity(client, userPath, "iron", 10);

            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertSuccessStatusCode();
            await AssertResourceQuantity(client, userPath, "carbon", 15);
            await AssertResourceQuantity(client, userPath, "iron", 5);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingBuilderOnBuiltStarportCost5Carbon10Iron()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));

            await AssertResourceQuantity(client, userPath, "carbon", 20);
            await AssertResourceQuantity(client, userPath, "iron", 10);

            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "builder"
            });
            await response.AssertSuccessStatusCode();
            await AssertResourceQuantity(client, userPath, "carbon", 15);
            await AssertResourceQuantity(client, userPath, "iron", 0);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutForInvalidUserReturns404()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));
            var response = await client.PostAsJsonAsync($"{userPath}z/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertStatusEquals(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutForInvalidBuildingReturns404()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));
            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}z/queue", new
            {
                type = "scout"
            });
            await response.AssertStatusEquals(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutOnMineReturns400()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMine(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));
            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertStatusEquals(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutOnUnBuiltStarportReturns400()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertStatusEquals(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutIfNotEnoughResourcesReturns400()
        {
            using var client = CreateClient();

            var (userPath, _, originalBuilding) = await BuildStarport(client);
            using var putResourceResponse = await PutResources(userPath, new
            {
                carbon = 0,
                iron = 0,
            });

            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertStatusEquals(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutIfNotEnoughIronDoesNotSpendCarbon()
        {
            using var client = CreateClient();

            var (userPath, _, originalBuilding) = await BuildStarport(client);
            using var putResourceResponse = await PutResources(userPath, new
            {
                carbon = 20,
                iron = 0,
            });

            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertStatusEquals(HttpStatusCode.BadRequest);
            await AssertResourceQuantity(client, userPath, "carbon", 20);
            await AssertResourceQuantity(client, userPath, "iron", 0);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutIfNotEnoughCarbonDoesNotSpendIron()
        {
            using var client = CreateClient();

            var (userPath, _, originalBuilding) = await BuildStarport(client);
            using var putResourceResponse = await PutResources(userPath, new
            {
                carbon = 0,
                iron = 10,
            });

            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertStatusEquals(HttpStatusCode.BadRequest);
            await AssertResourceQuantity(client, userPath, "carbon", 0);
            await AssertResourceQuantity(client, userPath, "iron", 10);
        }
    }
}
