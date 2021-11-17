using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shard.Shared.Core;

namespace Shard.Uni.Models
{
    public class Building
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string System { get; set; }
        public string Planet { get; set; }
        public bool? IsBuilt { get; set; }
        public string? EstimatedBuildTime { get; set; }
        public string? BuilderId { get; set; }
        public string ResourceCategory { get; set; }
        public Task Construction { get; set; }
        public CancellationTokenSource TokenSource;

        public Building(string id, string type, string system, string planet, string resourceCategory, string builderId, IClock clock)
        {
            Id = id;
            Type = type;
            System = system;
            Planet = planet;
            ResourceCategory = resourceCategory;
            IsBuilt = false;
            BuilderId = builderId;
            EstimatedBuildTime = clock.Now.AddMinutes(5.0).ToString("yyyy-MM-ddTHH:mm:sssK");
            TokenSource = new CancellationTokenSource();
        }

        public static List<string> GetBuildingTypes() => new List<string> { "mine", "starport" };

        public static List<string> GetResourcesTypes() => new List<string> { "solid", "liquid", "gaz" };

        public void StartConstruction(IClock clock, Planet planet, User user, string resourceCategory)
        {
            // We save this task for any controller/service to call it later
            Construction = Construct(clock, planet, user, resourceCategory);
        }

        private async Task Construct(IClock clock, Planet planet, User user, string resourceCategory)
        {
            await clock.Delay(new TimeSpan(0, 5, 0), TokenSource.Token);
            IsBuilt = true;
            EstimatedBuildTime = null;
            BuilderId = null;

            if(Type == "mine")
            {
                // Then every minute
                //      - +1 resource to owner
                //      - -1 resource from planet
                clock.CreateTimer(
                    _ =>
                    {
                        TokenSource.Token.ThrowIfCancellationRequested();
                        ResourceKind resource = planet.GetResourceToMine(resourceCategory);
                        try
                        {
                            planet.Mine(resource);
                        }
                        catch (NoResourcesAvailableException)
                        {
                            Console.WriteLine($"[WARNING] No more resources are available for resource {resource.ToString()}");
                            return;
                        }
                        user.ResourcesQuantity[resource] = user.ResourcesQuantity[resource] + 1;
                    },
                    null,
                    new TimeSpan(0, 1, 0), // starts the minute after building ...
                    new TimeSpan(0, 1, 0) // ... execute the code above every minute
                );
            }
        }

    }

    public class CreateBuilding
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string BuilderId { get; set; }
        public string ResourceCategory { get; set; }

        public CreateBuilding(string id, string type, string builderId, string resourceCategory)
        {
            Id = id;
            Type = type;
            BuilderId = builderId;
            ResourceCategory = resourceCategory;
        }
    }
}

