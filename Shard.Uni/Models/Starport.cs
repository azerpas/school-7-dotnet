using Shard.Shared.Core;
using System.Collections.Generic;

namespace Shard.Uni.Models
{
    public class Starport : Building
    {
        public List<QueueUnit> Queue { get; set; }

        public Starport(string id, string type, string system, string planet, string builderId, IClock clock, string resourceCategory)
            : base(id, type, system, planet, builderId, clock)
        {
            Queue = new List<QueueUnit>();
        }

        public async void AddToQueue(QueueUnit unit, User user)
        {
            Dictionary<ResourceKind, int> resourcesToConsume = GetStarportCosts()[unit.ToClass("", "").GetType()];
            if (user.HasEnoughResources(resourcesToConsume))
            {
                Queue.Add(unit);
                user.ConsumeResources(resourcesToConsume);
            }
            else
            {
                throw new NotEnoughResources();
            }
        }
    }
}
