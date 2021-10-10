using Microsoft.AspNetCore.Hosting; 
using Microsoft.AspNetCore.Mvc.Testing; 
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration; 
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; 
using Shard.Shared.Core;
using Shard.Shared.Web.IntegrationTests.Clock;
using Xunit; 
using Xunit.Abstractions; 
using Microsoft.AspNetCore.Builder;
using System.Linq; 
using System.Threading;
 
namespace Shard.Shared.Web.IntegrationTests 
{ 
    public abstract partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory> 
        : IClassFixture<TWebApplicationFactory> 
        where TEntryPoint : class 
        where TWebApplicationFactory: WebApplicationFactory<TEntryPoint> 
    { 
        private readonly WebApplicationFactory<TEntryPoint> factory; 
        private readonly FakeClock fakeClock = new();
 
        public BaseIntegrationTests(TWebApplicationFactory factory, ITestOutputHelper testOutputHelper) 
        { 
            this.factory = factory 
                .WithWebHostBuilder(builder => 
                { 
                    builder.ConfigureAppConfiguration(RemoveAllReloadOnChange); 
                    builder.ConfigureLogging( 
                        logging => logging.AddProvider(new XunitLoggerProvider(testOutputHelper))); 

                    builder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton<IClock>(fakeClock);	
                        services.AddSingleton<IStartupFilter>(fakeClock);
                    });
                }); 
        } 
 
        private void RemoveAllReloadOnChange(WebHostBuilderContext context, IConfigurationBuilder configuration) 
        { 
            foreach (var source in configuration.Sources.OfType<FileConfigurationSource>()) 
                source.ReloadOnChange = false; 
        }
    } 
} 
