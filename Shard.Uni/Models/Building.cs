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
        public string? ResourceCategory { get; set; }
        public List<QueueUnit> Queue { get; set; }
        public Task Construction { get; set; }
        public CancellationTokenSource TokenSource;

        public Building(string id, string type, string system, string planet, string resourceCategory, string builderId, IClock clock)
        {
            Id = id;
            Type = type;
            System = system;
            Planet = planet;
            ResourceCategory = Type == "mine" ? resourceCategory : null;
            IsBuilt = false;
            BuilderId = builderId;
            EstimatedBuildTime = clock.Now.AddMinutes(5.0).ToString("yyyy-MM-ddTHH:mm:sssK");
            TokenSource = new CancellationTokenSource();
        }

        public static List<string> GetBuildingTypes() => new List<string> { "mine", "starport" };

        public static List<string> GetResourcesTypes() => new List<string> { "solid", "liquid", "gaz" };

        public static Dictionary<string, Dictionary<ResourceKind, int>> GetStarportCosts() => new Dictionary<string, Dictionary<ResourceKind, int>>
        {
            {
                "scout",
                new Dictionary<ResourceKind, int> {
                    { ResourceKind.Carbon, 5 },
                    { ResourceKind.Iron, 5 }
                }
            },
            {
                "builder",
                new Dictionary<ResourceKind, int> {
                    { ResourceKind.Carbon, 5 },
                    { ResourceKind.Iron, 10 }
                }
            },
            {
                "fighter",
                new Dictionary<ResourceKind, int> {
                    { ResourceKind.Iron, 20 },
                    { ResourceKind.Aluminium, 10 }
                }
            },
            {
                "bomber",
                new Dictionary<ResourceKind, int> {
                    { ResourceKind.Iron, 30 },
                    { ResourceKind.Titanium, 10 }
                }
            },
            {
                "cruiser",
                new Dictionary<ResourceKind, int> {
                    { ResourceKind.Iron, 60 },
                    { ResourceKind.Gold, 20 }
                }
            }
        };

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

        public async void AddToQueue(QueueUnit unit, User user)
        {
            Dictionary<ResourceKind, int> resourcesToConsume = GetStarportCosts()[unit.Type];
            if (user.HasEnoughResources(resourcesToConsume))
            {
                Queue.Add(unit);
                user.ConsumeResources(resourcesToConsume);
            }
            else
            {
                throw new NotEnoughResources();
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

    public class QueueUnit
    {
        public string Type { get; set; }

        public QueueUnit(string type)
        {
            Type = type;
        }
    }

    public class NotEnoughResources : Exception
    {
        public NotEnoughResources() { }

        public NotEnoughResources(string message) : base(message) { }

        public NotEnoughResources(string message, Exception exception) : base(message, exception) { }
    }
}

