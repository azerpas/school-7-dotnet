using Shard.Shared.Core;

namespace Shard.Uni.Models
{
    public class Mine : Building
    {
        public string ResourceCategory { get; set; }

        public Mine(string id, string type, string system, string planet, string builderId, IClock clock, string resourceCategory)
            : base(id, type, system, planet, builderId, clock)
        {
            ResourceCategory = resourceCategory;
        }
    }
}
