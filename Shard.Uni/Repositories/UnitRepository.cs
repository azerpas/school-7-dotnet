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
    public class UnitRepository : IUnitRepository
    {
        private readonly IMongoCollection<Unit> collection;
        private readonly ILogger<UnitRepository> logger;

        public UnitRepository(MongoConnector mongo, ILogger<UnitRepository> logger)
        {
            RegisterClassMap();

            collection = mongo.Connector.GetCollection<Unit>("unit");
            this.logger = logger;
        }

        async Task<Unit> IUnitRepository.GetUnitById(string id)
        {
            return await(await collection.FindAsync(id)).SingleAsync();
        }

        async Task<IReadOnlyList<Unit>> IUnitRepository.GetUnits()
        {
            return await(await collection.FindAsync(Builders<Unit>.Filter.Empty)).ToListAsync();
        }

        public void RegisterClassMap()
        {
            BsonClassMap.RegisterClassMap<Unit>(map =>
            {
                map.AutoMap();
                map.MapIdProperty(unit => unit.Id);
                map.MapProperty(unit => unit.Type);
                map.MapProperty(unit => unit.Planet);
                map.MapProperty(unit => unit.System);
                map.MapProperty(unit => unit.DestinationPlanet);
                map.MapProperty(unit => unit.DestinationSystem);
                map.MapProperty(unit => unit.DestinationShard);
                map.MapProperty(unit => unit.EstimatedTimeOfArrival);
            });
            BsonSerializer.RegisterSerializer(new EnumSerializer<ResourceKind>(BsonType.String));
        }
    }
}