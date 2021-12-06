using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Shard.Shared.Core;
using Shard.Uni.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shard.Uni.Repositories
{
    public class SectorRepository : ISectorRepository
    {
        private readonly IMongoCollection<StarSystem> collection;
        private readonly Task<IReadOnlyList<StarSystem>> starSystemsTask;
        private readonly ILogger<SectorRepository> logger;

        public SectorRepository(MongoConnector mongo, MapGenerator generator, ILogger<SectorRepository> logger)
        {
            RegisterClassMap();

            collection = mongo.Connector.GetCollection<StarSystem>("starSystem");
            starSystemsTask = InitializeAsync(collection, generator);
            this.logger = logger;
        }

        public async Task<StarSystem> GetStarSystemById(string id)
        {
            return await (await collection.FindAsync(id)).SingleAsync();
        }

        public async Task<IReadOnlyList<StarSystem>> GetStarSystems()
        {
            return await (await collection.FindAsync(Builders<StarSystem>.Filter.Empty)).ToListAsync();
        }

        public async Task<IReadOnlyList<StarSystem>> InitializeAsync(IMongoCollection<StarSystem> collection, MapGenerator generator)
        {
            var starSystems = await (await collection.FindAsync(Builders<StarSystem>.Filter.Empty)).ToListAsync();
            if(starSystems.Count == 0)
            {
                logger.LogInformation("Initializing star systems");
                starSystems = (List<StarSystem>)generator.Generate().Systems;
                await collection.InsertManyAsync(starSystems);
                return starSystems;
            }
            else
            {
                logger.LogInformation("Loaded star systems");
                return starSystems;
            }
        }

        public void RegisterClassMap()
        {
            BsonClassMap.RegisterClassMap<StarSystem>(map =>
            {
                map.AutoMap();
                map.MapIdProperty(starSystem => starSystem.Name);
                map.MapProperty(starSystem => starSystem.Planets);
                map.MapCreator(starSytem => new StarSystem(starSytem.Name, starSytem.Planets));
            });
        }
    }
}
