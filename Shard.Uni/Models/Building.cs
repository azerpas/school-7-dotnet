using System;
using System.Collections.Generic;

namespace Shard.Uni.Models
{
    public class Building
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string System { get; set; }
        public string Planet { get; set; }

        public Building(string id, string type, string system, string planet)
        {
            Id = id;
            Type = type;
            System = system;
            Planet = planet;
        }

        public static List<string> getBuildingTypes() => new List<string> { "mine" };
    }

    public class CreateBuilding
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string BuilderId { get; set; }

        public CreateBuilding(string id, string type, string builderId)
        {
            Id = id;
            Type = type;
            BuilderId = builderId;
        }
    }
}

