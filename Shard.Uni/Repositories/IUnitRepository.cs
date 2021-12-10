using Shard.Uni.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shard.Uni.Repositories
{
    public interface IUnitRepository
    {
        void RegisterClassMap();
        Task<IReadOnlyList<Unit>> GetUnits();
        Task<Unit> GetUnitById(string id);
    }
}
