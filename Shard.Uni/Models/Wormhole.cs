using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Shard.Uni.Models
{
    public class Wormhole
    {
        public Dictionary<string, WormholeData> shards { get; set; }

        public Wormhole(IOptions<WormholeOptions> options): this(options.Value) { }

        public Wormhole(WormholeOptions value)
        {
            shards = value.shards;
        }
    }
    public class WormholeOptions
    {
        public Dictionary<string, WormholeData> shards { get; set; }

    }

    public class WormholeData
    {
        public string BaseUri { get; set; }
        public string System { get; set; }
        public string User { get; set; }
        public string SharedPassword { get; set; }

        public WormholeData(string baseUri, string system, string user, string sharedPassword)
        {
            BaseUri = baseUri;
            System = system; 
            User = user;
            SharedPassword = sharedPassword;
        }
    }
}
