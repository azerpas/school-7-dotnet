using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shard.Shared.Core;
using Shard.Uni.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication;
using Shard.Uni.Handlers;
using Shard.Uni.Models;

namespace Shard.Uni
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication("Basic").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);
            services.AddSingleton<MapGenerator>();
            services.Configure<MapGeneratorOptions>(options => options.Seed = "Uni");
            services.AddSingleton<Wormhole>();
            services.Configure<WormholeOptions>(
                options => options.shards = Configuration.GetSection("Wormholes").GetChildren()
                    .ToDictionary(
                        Section => Section.Key, 
                        Section => new WormholeData(
                            Section.GetValue<string>("baseUri"), 
                            Section.GetValue<string>("system"), 
                            Section.GetValue<string>("user"), 
                            Section.GetValue<string>("sharedPassword")
                        )
                    )
                );
            services.AddSingleton<MongoConnector>();
            services.Configure<MongoConnectorOptions>(Configuration.GetSection("Database"));
            services.AddSingleton<SectorService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<JumpService>();
            services.AddSingleton<Shared.Core.SystemClock>();
            services.AddHttpClient();
            services.AddTransient<JumpService>();
            services.AddTransient<IClock, Shared.Core.SystemClock>();
            services.AddHostedService<UnitsHostedService>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v7", new OpenApiInfo { Title = "Shard.Uni", Version = "v7" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v7/swagger.json", "Shard.Uni v7"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
