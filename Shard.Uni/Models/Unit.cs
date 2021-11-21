using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Shard.Shared.Core;

namespace Shard.Uni.Models
{
    public abstract class Unit
    {

        const int TimeToEnterPlanet = 15;
        const int TimeToLeavePlanet = 0;
        const int TimeToChangeSystem = 60;

        public string Id { get; }
        public string System { get; set; }
        public string? Planet { get; set; }
        public string DestinationPlanet { get; set; }
        public string DestinationSystem { get; set; }
        public string EstimatedTimeOfArrival { get; set; }

        public Unit(string system, string? planet)
        {
            Id = Guid.NewGuid().ToString();
            System = system;
            Planet = planet;
            DestinationSystem = system;
            DestinationPlanet = planet;
        }

        [JsonConstructorAttribute]
        public Unit(string id, string system, string? planet)
        {
            Id = id;
            System = system;
            Planet = planet;
            DestinationSystem = system;
            DestinationPlanet = planet;
        }

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

        public static Unit FromType(string type, string system, string? planet)
        {
            string id = Guid.NewGuid().ToString();
            switch (type)
            {
                case "builder":
                    return new Builder(id, system, planet);
                case "scout":
                    return new Scout(id, system, planet);
                case "bomber":
                    return new Bomber(id, system, planet);
                case "fighter":
                    return new Fighter(id, system, planet);
                case "cruiser":
                    return new Cruiser(id, system, planet);
                default:
                    throw new UnrecognizedUnit("Unrecognized type of Unit");
            }
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

    public class CreateUnitDto
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string System { get; set; }
        public string? Planet { get; set; }
        public string DestinationPlanet { get; set; }
        public string DestinationSystem { get; set; }
        public string EstimatedTimeOfArrival { get; set; }
        public int Health { get; set; }

        public Unit ToUnit()
        {
            switch (Type)
            {
                case "builder":
                    return new Builder(Id, System, Planet);
                case "scout":
                    return new Scout(Id, System, Planet);
                case "bomber":
                    return new Bomber(Id, System, Planet);
                case "fighter":
                    return new Fighter(Id, System, Planet);
                case "cruiser":
                    return new Cruiser(Id, System, Planet);
                default:
                    throw new UnrecognizedUnit("Unrecognized type of Unit");
            }
        }
    }

    public class UnrecognizedUnit : Exception
    {
        public UnrecognizedUnit()
        {
        }

        public UnrecognizedUnit(string message) : base(message)
        {
        }

        public UnrecognizedUnit(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

