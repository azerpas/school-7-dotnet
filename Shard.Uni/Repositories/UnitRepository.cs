using Shard.Uni.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shard.Uni.Repositories
{
    public class UnitRepository : IUnitRepository
    {
        Task<Unit> IUnitRepository.GetUnitById(string id)
        {
            throw new System.NotImplementedException();
        }

        Task<IReadOnlyList<Unit>> IUnitRepository.GetUnits()
        {
            throw new System.NotImplementedException();
        }

        public void RegisterClassMap()
        {
            throw new System.NotImplementedException();
        }
    }
}