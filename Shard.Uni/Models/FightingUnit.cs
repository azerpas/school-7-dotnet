using System;
using System.Collections.Generic;

namespace Shard.Uni.Models
{
    public abstract class FightingUnit : Unit
    {
        public int Health { get; set; }
        public int Damage { get; set; }

        public FightingUnit(string type, int health, int damage, string system, string? planet)
            : base(type, system, planet)
        {
            Health = health;
            Damage = damage;
        }

        public FightingUnit(string id, string type, int health, int damage, string system, string? planet)
            : base(id, type, system, planet)
        {
            Health = health;
            Damage = damage;
        }

        public static List<Type> GetFightingTypes => new List<Type> { typeof(Bomber), typeof(Fighter), typeof(Cruiser) };
    }
}

