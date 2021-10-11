using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Shard.Uni.Models
{

    public class Unit
    {
        public string Id { get; }
        public string Type { get; set; }
        public string System { get; set; }
        public string Planet { get; set; }

        public Unit(string type, string system, string planet)
        {
            Id = Guid.NewGuid().ToString();
            Type = type;
            System = system;
            Planet = planet;
        }

        [JsonConstructorAttribute]
        public Unit(string id, string type, string system, string planet)
        {
            Id = id;
            Type = type;
            System = system;
            Planet = planet;
        }

        public static List<string> getAuthorizedTypes()
        {
            return new List<string> { "scount", "builder" };
        }
    }

    public class UnitLocationDetailDto
    {
        public string System { get; set; }
        public string Planet { get; set; }
        public Dictionary<string, int> resourcesQuantity { get; set; }

        public UnitLocationDetailDto(string system, Planet planet)
        {
            System = system;
            Planet = planet.Name;
            resourcesQuantity = planet.ResourceQuantity.ToDictionary(Resource => Resource.Key.ToString().ToLower(), Resource => Resource.Value);
        }
    }
}

