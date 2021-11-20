using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Shard.Shared.Core;
using Shard.Uni.Services;

namespace Shard.Uni.Models
{
    public class Unit
    {
        const int TimeToEnterPlanet = 15;
        const int TimeToLeavePlanet = 0;
        const int TimeToChangeSystem = 60;

        public string Id { get; }
        public string Type { get; set; }
        public string System { get; set; }
        public string? Planet { get; set; }
        public string DestinationPlanet { get; set; }
        public string DestinationSystem { get; set; }
        public string EstimatedTimeOfArrival { get; set; }
        public int Health { get; set; }

        public Unit(string type, string system, string? planet)
        {
            Id = Guid.NewGuid().ToString();
            Type = type;
            System = system;
            Planet = planet;
            DestinationSystem = system;
            DestinationPlanet = planet;
            // TODO: replace by heritage
            Health = GetHealth();
        }

        [JsonConstructorAttribute]
        public Unit(string id, string type, string system, string? planet)
        {
            Id = id;
            Type = type;
            System = system;
            Planet = planet;
            Health = GetHealth();
        }

        public int GetHealth()
        {
            switch (Type)
            {
                case "fighter":
                    return 80;
                case "bomber":
                    return 100;
                default:
                    return 150; // TODO: to verify
            }
        }

        public static List<string> getAuthorizedTypes()
        {
            return new List<string> { "bomber", "fighter", "cruiser", "scout", "builder" };
        }

        public static List<string> GetFighterTypes()
        {
            return new List<string> { "bomber", "fighter", "cruiser" };
        }

#nullable enable
        public void MoveTo(string system, string? planet, IClock clock)
        {
            DateTime now = clock.Now;
            int timeToMove = TimeToLeavePlanet + TimeToEnterPlanet;
            bool sameSystem = System == system;
            if (!sameSystem)
            {
                timeToMove += TimeToChangeSystem;
            }
            now = now.AddSeconds(Convert.ToDouble(timeToMove));

            DestinationSystem = system;
            DestinationPlanet = planet;
            EstimatedTimeOfArrival = now.ToString("yyyy-MM-ddTHH:mm:sssK");
           
            clock.CreateTimer(move, this, timeToMove, 0);
        }

        public void move(object state)
        {
            Unit unit = (Unit)state;
            System = unit.DestinationSystem;
            Planet = unit.DestinationPlanet;
        }
    }

    public class UnitLocationDetailDto
    {
        public string System { get; set; }
        public string? Planet { get; set; }
        public Dictionary<string, int>? resourcesQuantity { get; set; }

        public UnitLocationDetailDto(string system, Planet? planet)
        {
            System = system;
            Planet = planet?.Name;
            resourcesQuantity = planet != null ? planet.ResourceQuantity.ToDictionary(Resource => Resource.Key.ToString().ToLower(), Resource => Resource.Value) : null;
        }
    }
}

