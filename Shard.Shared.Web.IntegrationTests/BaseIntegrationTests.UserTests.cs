using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Shard.Shared.Web.IntegrationTests
{
    public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
    {

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task CanGet404WhenQueryingUser()
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("users/42");
            await response.AssertStatusEquals(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task CanCreateUser()
        {
            using var client = factory.CreateClient();
            using var response = await client.PutAsJsonAsync("users/43", new
            {
                id = "43",
                pseudo = "johny"
            });
            await response.AssertSuccessStatusCode();

            var user = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal("43", user["id"].Value<string>());
            Assert.Equal("johny", user["pseudo"].Value<string>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task CreatingUserWithInconsistentIdFails()
        {
            using var client = factory.CreateClient();
            using var response = await client.PutAsJsonAsync("users/44", new
            {
                id = "45",
                pseudo = "johny"
            });
            await response.AssertStatusEquals(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task CreatingUserWithLackOfBodyFails()
        {
            using var client = factory.CreateClient();
            using var response = await client.PutAsJsonAsync<object>("users/46", null);
            await response.AssertStatusEquals(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task CreatingUserWithInvalidIdFails()
        {
            using var client = factory.CreateClient();
            using var response = await client.PutAsJsonAsync("users/'", new
            {
                id = "'",
                pseudo = "johny"
            });
            await response.AssertStatusCodeAmong(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task CanFetchCreatedUser()
        {
            using var client = factory.CreateClient();
            using var userCreationResponse = await client.PutAsJsonAsync("users/47", new
            {
                id = "47",
                pseudo = "johny"
            });
            await userCreationResponse.AssertSuccessStatusCode();

            using var getUserResponse = await client.GetAsync("users/47");
            await getUserResponse.AssertSuccessStatusCode();

            var user = await getUserResponse.Content.ReadAsAsync<JObject>();
            Assert.NotNull(user["id"]);
            Assert.Equal(JTokenType.String, user["id"].Type);
            Assert.Equal("47", user["id"].Value<string>());

            Assert.NotNull(user["pseudo"]);
            Assert.Equal(JTokenType.String, user["pseudo"].Type);
            Assert.Equal("johny", user["pseudo"].Value<string>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public async Task CanFetchResourcesFromNewlyCreatedUser()
        {
            using var client = factory.CreateClient();
            using var getUserResponse = await client.GetAsync(await CreateNewUserPath());

            var user = await getUserResponse.Content.ReadAsAsync<JObject>();
            AssertResourcesQuantity(user);
        }

        [Theory]
        [InlineData("aluminium", 0)]
        [InlineData("carbon", 20)]
        [InlineData("gold", 0)]
        [InlineData("iron", 10)]
        [InlineData("oxygen", 50)]
        [InlineData("titanium", 0)]
        [InlineData("water", 50)]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public async Task GivesBasicResourcesToNewUser(string resourceName, int resourceQuantity)
        {
            using var client = factory.CreateClient();
            using var getUserResponse = await client.GetAsync(await CreateNewUserPath());

            var user = await getUserResponse.Content.ReadAsAsync<JObject>();
            Assert.Equal(resourceQuantity, user["resourcesQuantity"][resourceName].Value<int>());
        }
    }
}
