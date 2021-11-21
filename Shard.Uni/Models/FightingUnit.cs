using System;
namespace Shard.Uni.Models
{
    public class FightingUnit : Unit
    {

        public int Health { get; set; }
        public int Damage { get; set; }

        public FightingUnit(int health, int damage, string system, string? planet)
            : base(system, planet)
        {
            Health = health;
            Damage = damage;
        }

        public FightingUnit(string id, int damage, int health, string system, string? planet)
            : base(id, system, planet)
        {
            Health = health;
            Damage = damage;
        }
    }
}

