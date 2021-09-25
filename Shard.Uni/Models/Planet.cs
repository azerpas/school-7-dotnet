using System.Collections.Generic;
using Shard.Shared.Core;

namespace Shard.Uni.Models
{
    public class Planet
    {
        public string Name { get; }
        public int Size { get; }

        public Dictionary<ResourceKind, int> ResourceQuantity { get; }

        public Planet(string name, int size, Dictionary<ResourceKind, int> resourceQuantity)
        {
            Name = name;
            Size = size;
            ResourceQuantity = resourceQuantity;
        }
    }
}
