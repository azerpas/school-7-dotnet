using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shard.Shared.Core;

namespace Shard.Uni.Models
{
    public abstract class Building
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string System { get; set; }
        public string Planet { get; set; }
        public bool? IsBuilt { get; set; }
        public string? EstimatedBuildTime { get; set; }
        public string? BuilderId { get; set; }
        public Task Construction { get; set; }
        public CancellationTokenSource TokenSource;

        public Building(string id, string type, string system, string planet, string builderId, IClock clock)
        {
            Id = id;
            Type = type;
            System = system;
            Planet = planet;
            IsBuilt = false;
            BuilderId = builderId;
            EstimatedBuildTime = clock.Now.AddMinutes(5.0).ToString("yyyy-MM-ddTHH:mm:sssK");
            TokenSource = new CancellationTokenSource();
        }

        public static List<string> GetBuildingTypes() => new List<string> { "mine", "starport" };

        public static List<string> GetResourcesTypes() => new List<string> { "solid", "liquid", "gaz" };

        public static Dictionary<Type, Dictionary<ResourceKind, int>> GetStarportCosts() => new Dictionary<Type, Dictionary<ResourceKind, int>>
        {
            {
                typeof(Scout),
                new Dictionary<ResourceKind, int> {
                    { ResourceKind.Carbon, 5 },
                    { ResourceKind.Iron, 5 }
                }
            },
            {
                typeof(Builder),
                new Dictionary<ResourceKind, int> {
                    { ResourceKind.Carbon, 5 },
                    { ResourceKind.Iron, 10 }
                }
            },
            {
                typeof(Fighter),
                new Dictionary<ResourceKind, int> {
                    { ResourceKind.Iron, 20 },
                    { ResourceKind.Aluminium, 10 }
                }
            },
            {
                typeof(Bomber),
                new Dictionary<ResourceKind, int> {
                    { ResourceKind.Iron, 30 },
                    { ResourceKind.Titanium, 10 }
                }
            },
            {
                typeof(Cruiser),
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

        public Building ToClass(string system, string planet, IClock clock)
        {
            switch (Type)
            {
                case "mine":
                    return new Mine(Id, Type, system, planet, BuilderId, clock, ResourceCategory);
                case "starport":
                    return new Starport(Id, Type, system, planet, BuilderId, clock, ResourceCategory);
                default:
                    throw new UnrecognizedBuilding("Unrecognized type of Building");
            }
        }
    }

    public class BuildingDto
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string System { get; set; }
        public string Planet { get; set; }
        public bool? IsBuilt { get; set; }
        public string? EstimatedBuildTime { get; set; }
        public string? BuilderId { get; set; }
        public Task Construction { get; set; }
        public CancellationTokenSource TokenSource;

        public BuildingDto(string id, string type, string system, string planet, bool isBuilt, string estimatedBuildTime, string builderId, Task construction, CancellationTokenSource tokenSource)
        {
            Id = id;
            Type = type;
            System = system;
            Planet = planet;
            IsBuilt = isBuilt;
            EstimatedBuildTime = estimatedBuildTime;
            BuilderId = builderId;
            Construction = construction;
            TokenSource = tokenSource;
        }

        public BuildingDto(Building building)
        {
            Id = building.Id;
            Type = building.Type;
            System = building.System;
            Planet = building.Planet;
            IsBuilt = building.IsBuilt;
            EstimatedBuildTime = building.EstimatedBuildTime;
            BuilderId = building.BuilderId;
            Construction = building.Construction;
            TokenSource = building.TokenSource;
        }
    }

    public class QueueUnit
    {
        public string Type { get; set; }

        public QueueUnit(string type)
        {
            Type = type;
        }

        public Unit ToClass(string system, string planet)
        {
            switch (Type)
            {
                case "builder":
                    return new Builder(system, planet);
                case "scout":
                    return new Scout(system, planet);
                case "bomber":
                    return new Bomber(system, planet);
                case "fighter":
                    return new Fighter(system, planet);
                case "cruiser":
                    return new Cruiser(system, planet);
                default:
                    throw new UnrecognizedUnit("Unrecognized type of Unit");
            }
        }
    }

    public class NotEnoughResources : Exception
    {
        public NotEnoughResources() { }

        public NotEnoughResources(string message) : base(message) { }

        public NotEnoughResources(string message, Exception exception) : base(message, exception) { }
    }

    public class UnrecognizedBuilding : Exception
    {
        public UnrecognizedBuilding() { }

        public UnrecognizedBuilding(string message) : base(message) { }

        public UnrecognizedBuilding(string message, Exception exception) : base(message, exception) { }
    }
}

