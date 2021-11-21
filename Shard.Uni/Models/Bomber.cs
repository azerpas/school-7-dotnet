using System;
namespace Shard.Uni.Models
{
    public class Bomber : FightingUnit
    {
        public Bomber(string system, string? planet)
            : base("bomber", Constants.Fighters.Health.Bomber, Constants.Fighters.Damage.Bomber, Constants.Fighters.Timeout.Bomber, system, planet)
        { }

        public Bomber(string id, string system, string? planet)
            : base(id, "bomber", Constants.Fighters.Health.Bomber, Constants.Fighters.Damage.Bomber, Constants.Fighters.Timeout.Bomber, system, planet)
        { }
    }
}

