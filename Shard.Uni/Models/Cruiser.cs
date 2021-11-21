using System;
namespace Shard.Uni.Models
{
    public class Cruiser : FightingUnit
    {
        public Cruiser(string system, string? planet)
            : base(Constants.Fighters.Health.Cruiser, Constants.Fighters.Damage.Cruiser, system, planet)
        { }

        public Cruiser(string id, string system, string? planet)
            : base(id, Constants.Fighters.Health.Cruiser, Constants.Fighters.Damage.Cruiser, system, planet)
        { }
    }
}

