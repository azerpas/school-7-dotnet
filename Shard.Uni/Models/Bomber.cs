using System;
namespace Shard.Uni.Models
{
    public class Bomber : FightingUnit
    {
        public Bomber(string system, string? planet)
            : base(Constants.Fighters.Health.Bomber, Constants.Fighters.Damage.Bomber, system, planet)
        { }

        public Bomber(string id, string system, string? planet)
            : base(id, Constants.Fighters.Health.Bomber, Constants.Fighters.Damage.Bomber, system, planet)
        { }
    }
}

