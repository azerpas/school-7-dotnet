using System;
using System.Collections.Generic;
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
        public string EstimatedBuildTime { get; set; }
        public string ResourceCategory { get; set; }

        public Building(string id, string type, string system, string planet, string resourceCategory, IClock clock)
        {
            Id = id;
            Type = type;
            System = system;
            Planet = planet;
            ResourceCategory = resourceCategory;
            IsBuilt = false;
            EstimatedBuildTime = clock.Now.AddMinutes(5.0).ToString("yyyy-MM-ddTHH:mm:sssK");
        }

        public static List<string> GetBuildingTypes() => new List<string> { "mine" };

        public static List<string> GetResourcesTypes() => new List<string> { "solid", "liquid", "gaz" };

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

