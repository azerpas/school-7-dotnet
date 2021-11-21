using System;
namespace Shard.Uni.Models
{
    public class Fighter : FightingUnit
    {
        public Fighter(string system, string? planet)
            : base("fighter", Constants.Fighters.Health.Fighter, Constants.Fighters.Damage.Fighter, system, planet)
        { }

        public Fighter(string id, string system, string? planet)
            : base(id, "fighter", Constants.Fighters.Health.Fighter, Constants.Fighters.Damage.Fighter, system, planet)
        { }
    }
}

