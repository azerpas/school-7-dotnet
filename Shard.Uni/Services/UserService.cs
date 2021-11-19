using Shard.Uni.Models;
using System.Collections.Generic;
using System.Linq;

namespace Shard.Uni.Services
{
    public class UserService
    {
        public List<User> Users { get; set; } = new();
        public Dictionary<string, List<Unit>> Units { get; set; } = new();
        public Dictionary<string, List<Building>> Buildings { get; set; } = new();

        public UserService()
        {
            Users = new List<User>();
            Units = new Dictionary<string, List<Unit>>();
            Buildings = new Dictionary<string, List<Building>>();
        }

        public List<Unit> GetAllUnits()
        {
            return Units
                .Select((KeyValuePair<string, List<Unit>> keyValue) => keyValue.Value)
                .SelectMany(Units => Units)
                .ToList();
        }

        public List<Unit> GetFighterUnits()
        {
            return GetAllUnits()
                .FindAll(unit => Unit.GetFighterTypes().Contains(unit.Type));
        }

        public List<Unit> GetUnitsOnPlanet(string planet)
        {
            return GetAllUnits()
                .FindAll(Unit => Unit.Planet == planet);
        }

        public List<Unit> GetUnitsInSystem(string system)
        {
            return GetAllUnits()
                .FindAll(Unit => Unit.System == system);
        }
    }
}

