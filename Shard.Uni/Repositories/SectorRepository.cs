using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Shard.Shared.Core;
using Shard.Uni.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
                starSystems = generator.Generate().Systems.ToList().Select(
                    System => new StarSystem(
                        System.Name,
                        System.Planets.Select(
                            Planet => new Planet(
                                Planet.Name,
                                Planet.Size,
                                Planet.ResourceQuantity.Select(
                                    Resource => Resource
                                ).ToDictionary(
                                    Resource => Resource.Key,
                                    Resource => Resource.Value
                                )
                            )
                        ).ToList()
                    )
                ).ToList();
                try
                {
                    await collection.InsertManyAsync(starSystems);
                }
                catch (MongoException ex)
                {
                    logger.LogError($"Mongo error while persisting data: {ex.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error while persisting data: {ex.Message}");
                    throw;
                }
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
            BsonSerializer.RegisterSerializer(new EnumSerializer<ResourceKind>(BsonType.String));
        }
    }
}
