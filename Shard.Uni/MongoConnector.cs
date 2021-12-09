using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Shard.Uni
{
    public class MongoConnector
    {

        public IMongoDatabase Connector { get; }

        public MongoConnector(IOptions<MongoConnectorOptions> options, ILogger<MongoConnector> logger)
        {
            var settings = options.Value;
            var client = new MongoClient(settings.ConnectionString);
            Connector = client.GetDatabase(settings.Name);
            var pack = new ConventionPack
            {
                new EnumRepresentationConvention(MongoDB.Bson.BsonType.String)
            };
            ConventionRegistry.Register("EnumStringConvention", pack, t => true);
            logger.LogInformation($"Database mongo is {settings.ConnectionString} - {settings.Name}");
        }
    }

    public class MongoConnectorOptions
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
    }
}
