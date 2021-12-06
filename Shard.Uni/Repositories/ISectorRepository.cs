using MongoDB.Driver;
using Shard.Shared.Core;
using Shard.Uni.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shard.Uni.Repositories
{
    public interface ISectorRepository
    {
        Task<IReadOnlyList<StarSystem>> InitializeAsync(IMongoCollection<StarSystem> collection, MapGenerator generator);
        void RegisterClassMap();
        Task<IReadOnlyList<StarSystem>> GetStarSystems();
        Task<StarSystem> GetStarSystemById(string id);
    }
}
