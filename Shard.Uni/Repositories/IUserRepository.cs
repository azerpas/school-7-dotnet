using MongoDB.Driver;
using Shard.Uni.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shard.Uni.Repositories
{
    public interface IUserRepository
    {
        Task<IReadOnlyList<User>> GetUsers();
        Task<User> GetById(int id);

        void CreateUser(User user);
    }
}
