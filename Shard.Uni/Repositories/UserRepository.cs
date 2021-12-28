using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Shard.Shared.Core;
using Shard.Uni.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shard.Uni.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> collection;
        private readonly ILogger<UserRepository> logger;

        public UserRepository(MongoConnector mongo, ILogger<UserRepository> logger)
        {
            RegisterClassMap();

            collection = mongo.Connector.GetCollection<User>("user");
            this.logger = logger;
        }

        public async void CreateUser(User user)
        {
            await collection.InsertOneAsync(user);
        }

        Task<User> IUserRepository.GetById(int id)
        {
            throw new System.NotImplementedException();
        }

        Task<IReadOnlyList<User>> IUserRepository.GetUsers()
        {
            throw new System.NotImplementedException();
        }

        public void RegisterClassMap()
        {
            BsonClassMap.RegisterClassMap<User>(map =>
            {
                map.AutoMap();
                map.MapIdProperty(user => user.Id);
                map.MapProperty(user => user.DateOfCreation);
                map.MapProperty(user => user.Pseudo);
            });
            // BsonSerializer.RegisterSerializer(new EnumSerializer<ResourceKind>(BsonType.String));
        }
    }
}
