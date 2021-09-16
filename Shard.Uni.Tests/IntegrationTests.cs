using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Shard.Shared.Web.IntegrationTests;
using Xunit.Abstractions;

namespace Shard.Uni.Tests
{
    public class IntegrationTests : BaseIntegrationTests<Startup, WebApplicationFactory<Startup>>
    {
        public IntegrationTests(WebApplicationFactory<Startup> factory, ITestOutputHelper testOutputHelper)
            : base(factory, testOutputHelper)
        {
        }

    }
}