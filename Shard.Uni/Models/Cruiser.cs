using System;
namespace Shard.Uni.Models
{
    public class Cruiser : FightingUnit
    {
        public Cruiser(string system, string? planet)
            : base("cruiser", Constants.Fighters.Health.Cruiser, Constants.Fighters.Damage.Cruiser, Constants.Fighters.Timeout.Cruiser, system, planet)
        { }

        public Cruiser(string id, string system, string? planet)
            : base(id, "cruiser", Constants.Fighters.Health.Cruiser, Constants.Fighters.Damage.Cruiser, Constants.Fighters.Timeout.Cruiser, system, planet)
        { }
    }
}

