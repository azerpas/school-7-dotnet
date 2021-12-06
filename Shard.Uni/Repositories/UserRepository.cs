using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Shard.Uni.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shard.Uni.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> collection;
        private readonly ILogger<UserRepository> logger;

        Task<User> IUserRepository.GetById(int id)
        {
            throw new System.NotImplementedException();
        }

        Task<IReadOnlyList<User>> IUserRepository.GetUsers()
        {
            throw new System.NotImplementedException();
        }
    }
}
