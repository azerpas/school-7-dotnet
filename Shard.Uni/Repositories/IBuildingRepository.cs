using MongoDB.Driver;
using Shard.Uni.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shard.Uni.Repositories
{
    public interface IBuildingRepository
    {
        void RegisterClassMap();
        Task<IReadOnlyList<Building>> GetBuildings();
        Task<Building> GetBuildingById(string id);
        void CreateBuilding(Building building);
    }
}
