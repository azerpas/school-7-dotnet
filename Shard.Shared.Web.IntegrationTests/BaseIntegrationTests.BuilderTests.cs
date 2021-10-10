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
        private Task<JObject> GetBuilder(string userPath)
            => GetSingleUnitOfType(userPath, "builder");

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public Task CreatingUserCreatesBuilder()
            => CreatingUserCreatesOneUnitOfType("builder");

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
        public Task GettingBuilderStatusById()
            => GettingUnitStatusById("builder");

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public Task GettingBuilderStatusWithWrongIdReturns404()
            => GettingUnitStatusWithWrongIdReturns404("builder");

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public Task MoveBuilderToOtherSystem()
            => MoveUnitToOtherSystem("builder");

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public Task MoveBuilderToPlanet()
            => MoveUnitToPlanet("builder");

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
                destinationSystem = currentSystem,
                destinationPlanet
            });
            await moveResponse.AssertSuccessStatusCode();

            fakeClock.Advance(new TimeSpan(0, 0, 15));

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
