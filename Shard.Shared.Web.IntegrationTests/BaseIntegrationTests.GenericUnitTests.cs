﻿using Newtonsoft.Json.Linq;
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
        private async Task<JObject> GetSingleUnitOfType(string userPath, string unitType)
        {
            using var client = factory.CreateClient();
            using var unitsResponse = await client.GetAsync($"{userPath}/units");
            await unitsResponse.AssertSuccessStatusCode();

            var units = (await unitsResponse.Content.ReadAsAsync<JArray>())
                .Where(unit => unit["type"].Value<string>() == unitType)
                .ToArray();
            Assert.Single(units);
            return units[0].Value<JObject>();
        }

        private async Task CreatingUserCreatesOneUnitOfType(string unitType)
        {
            var unit = await GetSingleUnitOfType(await CreateNewUserPath(), unitType);
            Assert.NotNull(unit["type"]);
            Assert.Equal(JTokenType.String, unit["type"].Type);
            Assert.Equal(unitType, unit["type"].Value<string>());
        }

        public async Task GettingUnitStatusById(string unitType)
        {
            var userPath = await CreateNewUserPath();
            var unit = await GetSingleUnitOfType(userPath, unitType);
            var unitId = unit["id"].Value<string>();

            using var client = factory.CreateClient();
            using var response = await client.GetAsync($"{userPath}/units/{unitId}");
            await response.AssertSuccessStatusCode();

            var unit2 = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal(unit.ToString(), unit2.ToString());
        }

        public async Task GettingUnitStatusWithWrongIdReturns404(string unitType)
        {
            var userPath = await CreateNewUserPath();
            var unit = await GetSingleUnitOfType(userPath, unitType);
            var unitId = unit["id"].Value<string>();

            using var client = factory.CreateClient();
            using var response = await client.GetAsync($"{userPath}/units/{unitId}z");
            await response.AssertStatusEquals(HttpStatusCode.NotFound);
        }

        public async Task MoveUnitToOtherSystem(string unitType)
        {
            var userPath = await CreateNewUserPath();
            var unit = await GetSingleUnitOfType(userPath, unitType);
            var unitId = unit["id"].Value<string>();

            var currentSystem = unit["system"].Value<string>();
            var destinationSystem = await GetRandomSystemOtherThan(currentSystem);

            unit["destinationSystem"] = destinationSystem;

            using var client = factory.CreateClient();
            using var response = await client.PutAsJsonAsync($"{userPath}/units/{unitId}", unit);
            await response.AssertSuccessStatusCode();

            var unitAfterMove = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal(unitId, unitAfterMove["id"].Value<string>());
            Assert.Equal(destinationSystem, unitAfterMove["destinationSystem"].Value<string>());
        }

        private async Task<string> GetRandomSystemOtherThan(string systemName)
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("systems");
            await response.AssertSuccessStatusCode();

            var systems = await response.Content.ReadAsAsync<JArray>();
            var system = systems.FirstOrDefault(system => system["name"].Value<string>() != systemName);
            Assert.NotNull(system);

            return system["name"].Value<string>();
        }

        public async Task MoveUnitToPlanet(string unitType)
        {
            var userPath = await CreateNewUserPath();
            var unit = await GetSingleUnitOfType(userPath, unitType);
            var unitId = unit["id"].Value<string>();

            var currentSystem = unit["system"].Value<string>();
            var destinationPlanet = (await GetSomePlanetInSystem(currentSystem));

            unit["destinationSystem"] = currentSystem;
            unit["destinationPlanet"] = destinationPlanet;

            using var client = factory.CreateClient();
            using var response = await client.PutAsJsonAsync($"{userPath}/units/{unitId}", unit);
            await response.AssertSuccessStatusCode();

            var unitAfterMove = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal(unitId, unitAfterMove["id"].Value<string>());
            Assert.Equal(currentSystem, unitAfterMove["destinationSystem"].Value<string>());
            Assert.Equal(destinationPlanet, unitAfterMove["destinationPlanet"].Value<string>());
        }

        private async Task<string> GetSomePlanetInSystem(string systemName)
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("systems");
            await response.AssertSuccessStatusCode();

            var systems = await response.Content.ReadAsAsync<JArray>();
            var system = systems.SingleOrDefault(system => system["name"].Value<string>() == systemName);
            Assert.NotNull(system);

            var planet = system["planets"].FirstOrDefault();
            Assert.NotNull(planet);

            return planet["name"].Value<string>();
        }

        private async Task<(string, string)> DirectUnitToPlanet(HttpClient client, string unitType)
        {
            var userPath = await CreateNewUserPath();
            var unit = await GetSingleUnitOfType(userPath, unitType);
            var unitId = unit["id"].Value<string>();
            var currentSystem = unit["system"].Value<string>();
            var destinationPlanet = await GetSomePlanetInSystem(currentSystem);

            unit["destinationSystem"] = currentSystem;
            unit["destinationPlanet"] = destinationPlanet;

            using var moveResponse = await client.PutAsJsonAsync($"{userPath}/units/{unitId}", unit);
            await moveResponse.AssertSuccessStatusCode();

            return (userPath, unitId);
        }

        private async Task<(string, JObject)> SendUnitToPlanet(HttpClient client, string unitType)
        {
            var (userPath, unitId) = await DirectUnitToPlanet(client, unitType);

            await fakeClock.Advance(new TimeSpan(0, 0, 15));

            using var afterMoveResponse = await client.GetAsync($"{userPath}/units/{unitId}");
            return (userPath, await afterMoveResponse.Content.ReadAsAsync<JObject>());
        }

        public async Task GetUnit_IfMoreThan2secAway_DoesNotWait(string unitType)
        {
            using var client = factory.CreateClient();
            var (userPath, unitId) = await DirectUnitToPlanet(client, unitType);

            await fakeClock.Advance(new TimeSpan(0, 0, 13) - TimeSpan.FromTicks(1));

            var requestTask = client.GetAsync($"{userPath}/units/{unitId}");
            var delayTask = Task.Delay(500);
            var firstToSucceed = await Task.WhenAny(requestTask, delayTask);

            Assert.Same(requestTask, firstToSucceed);

            using var response = await requestTask;
            await response.AssertSuccessStatusCode();
            var unitAfterMove = await response.Content.ReadAsAsync<JObject>();
            Assert.Null(unitAfterMove["planet"].Value<string>());
        }

        public async Task GetUnit_IfLessOrEqualThan2secAway_Waits(string unitType)
        {
            using var client = factory.CreateClient();
            var (userPath, unitId) = await DirectUnitToPlanet(client, unitType);

            await fakeClock.Advance(new TimeSpan(0, 0, 13));

            var requestTask = client.GetAsync($"{userPath}/units/{unitId}");
            var delayTask = Task.Delay(500);
            var firstToSucceed = await Task.WhenAny(requestTask, delayTask);

            Assert.Same(delayTask, firstToSucceed);
        }

        public async Task GetUnit_IfLessOrEqualThan2secAway_WaitsUntilArrived(string unitType)
        {
            using var client = factory.CreateClient();
            var (userPath, unitId) = await DirectUnitToPlanet(client, unitType);

            await fakeClock.Advance(new TimeSpan(0, 0, 13));

            var requestTask = client.GetAsync($"{userPath}/units/{unitId}");
            await Task.Delay(500);

            await fakeClock.Advance(new TimeSpan(0, 0, 2));

            var delayTask = Task.Delay(500);
            var firstToSucceed = await Task.WhenAny(requestTask, delayTask);

            Assert.Same(requestTask, firstToSucceed);

            using var response = await requestTask;
            await response.AssertSuccessStatusCode();
            var unitAfterMove = await response.Content.ReadAsAsync<JObject>();
            Assert.NotNull(unitAfterMove["planet"].Value<string>());
        }
    }
}
