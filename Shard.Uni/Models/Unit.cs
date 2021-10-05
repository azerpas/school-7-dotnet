using System;
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
    }
}

