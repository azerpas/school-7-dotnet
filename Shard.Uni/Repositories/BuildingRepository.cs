using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Shard.Shared.Core;
using Shard.Uni.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shard.Uni.Repositories
{
    public class BuildingRepository : IBuildingRepository
    {
        private readonly IMongoCollection<Building> collection;
        private readonly ILogger<BuildingRepository> logger;

        public BuildingRepository(MongoConnector mongo, ILogger<BuildingRepository> logger)
        {
            RegisterClassMap();

            collection = mongo.Connector.GetCollection<Building>("building");
            this.logger = logger;
        }

        public async Task<Building> GetBuildingById(string id)
        {
            return await(await collection.FindAsync(id)).SingleAsync();
        }

        public async Task<IReadOnlyList<Building>> GetBuildings()
        {
            return await(await collection.FindAsync(Builders<Building>.Filter.Empty)).ToListAsync();
        }

        public void RegisterClassMap()
        {
            BsonClassMap.RegisterClassMap<Building>(map =>
            {
                map.AutoMap();
                map.MapIdProperty(building => building.Id);
                map.MapProperty(building => building.Type);
                map.MapProperty(building => building.Planet);
                map.MapProperty(building => building.System);
                map.MapProperty(building => building.System);
                map.MapProperty(building => building.EstimatedBuildTime);
                map.MapProperty(building => building.IsBuilt);
            });
            BsonSerializer.RegisterSerializer(new EnumSerializer<ResourceKind>(BsonType.String));
        }
    }
}
