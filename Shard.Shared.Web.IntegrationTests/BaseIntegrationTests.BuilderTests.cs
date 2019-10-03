using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Shard.Shared.Web.IntegrationTests
{
    public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
    {
        private async Task<JObject> GetBuilder(string userPath)
        {
            using var client = factory.CreateClient();
            using var unitsResponse = await client.GetAsync($"{userPath}/units");
            await unitsResponse.AssertSuccessStatusCode();

            var units = (await unitsResponse.Content.ReadAsAsync<JArray>())
                .Where(unit => unit["type"].Value<string>() == "builder")
                .ToArray();
            Assert.Single(units);
            return units[0].Value<JObject>();
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public async Task CreatingUserCreatesBuilder()
        {
            var unit = await GetBuilder(await CreateNewUserPath());
            Assert.NotNull(unit["type"]);
            Assert.Equal(JTokenType.String, unit["type"].Type);
            Assert.Equal("builder", unit["type"].Value<string>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public async Task CreatingUserCreatesBuilderInSameSystemThanScout()
        {
            var userPath = await CreateNewUserPath();
            var scout = await GetScout(userPath);
            var builder = await GetBuilder(userPath);
            Assert.NotNull(builder["system"]);
            Assert.Equal(JTokenType.String, builder["system"].Type);
            Assert.Equal(scout["system"].Value<string>(), builder["system"].Value<string>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public async Task GettingBuilderStatusById()
        {
            var userPath = await CreateNewUserPath();
            var unit = await GetBuilder(userPath);
            var unitId = unit["id"].Value<string>();

            using var client = factory.CreateClient();
            using var response = await client.GetAsync($"{userPath}/units/{unitId}");
            await response.AssertSuccessStatusCode();

            var unit2 = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal(unit.ToString(), unit2.ToString());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public async Task GettingBuilderStatusWithWrongIdReturns404()
        {
            var userPath = await CreateNewUserPath();
            var unit = await GetBuilder(userPath);
            var unitId = unit["id"].Value<string>();

            using var client = factory.CreateClient();
            using var response = await client.GetAsync($"{userPath}/units/{unitId}z");
            await response.AssertStatusEquals(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public async Task MoveBuilderToOtherSystem()
        {
            var userPath = await CreateNewUserPath();
            var unit = await GetBuilder(userPath);
            var unitId = unit["id"].Value<string>();

            var currentSystem = unit["system"].Value<string>();
            var destinationSystem = await GetRandomSystemOtherThan(currentSystem);

            using var client = factory.CreateClient();
            using var response = await client.PutAsJsonAsync($"{userPath}/units/{unitId}", new
            {
                id = unitId,
                system = destinationSystem
            });
            await response.AssertSuccessStatusCode();

            var unitAfterMove = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal(unitId, unitAfterMove["id"].Value<string>());
            Assert.Equal(destinationSystem, unitAfterMove["system"].Value<string>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public async Task MoveBuilderToPlanet()
        {
            var userPath = await CreateNewUserPath();
            var unit = await GetBuilder(userPath);
            var unitId = unit["id"].Value<string>();

            var currentSystem = unit["system"].Value<string>();
            var destinationPlanet = (await GetSomePlanetInSystem(currentSystem));

            using var client = factory.CreateClient();
            using var response = await client.PutAsJsonAsync($"{userPath}/units/{unitId}", new
            {
                id = unitId,
                system = currentSystem,
                planet = destinationPlanet
            });
            await response.AssertSuccessStatusCode();

            var unitAfterMove = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal(unitId, unitAfterMove["id"].Value<string>());
            Assert.Equal(currentSystem, unitAfterMove["system"].Value<string>());
            Assert.Equal(destinationPlanet, unitAfterMove["planet"].Value<string>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public async Task AskingCurrentLocationsOfBuilderDoesNotReturnDetails()
        {
            var userPath = await CreateNewUserPath();
            var unit = await GetBuilder(userPath);
            var unitId = unit["id"].Value<string>();

            var currentSystem = unit["system"].Value<string>();
            var destinationPlanet = await GetSomePlanetInSystem(currentSystem);

            using var client = factory.CreateClient();
            using var moveResponse = await client.PutAsJsonAsync($"{userPath}/units/{unitId}", new
            {
                id = unitId,
                system = currentSystem,
                planet = destinationPlanet
            });
            await moveResponse.AssertSuccessStatusCode();

            using var scoutingResponse = await client.GetAsync($"{userPath}/units/{unitId}/location");
            await scoutingResponse.AssertSuccessStatusCode();

            var location = await scoutingResponse.Content.ReadAsAsync<JObject>();
            Assert.Equal(currentSystem, location["system"].Value<string>());
            Assert.Equal(destinationPlanet, location["planet"].Value<string>());

            Assert.True(location["resourcesQuantity"] == null
                || location["resourcesQuantity"].Type == JTokenType.Null);
        }
    }
}
