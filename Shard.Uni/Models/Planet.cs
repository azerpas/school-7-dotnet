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

    public class PlanetDetailDto
    {
        public string Name { get; set; }
        public int Size { get; set; }

        public PlanetDetailDto(string name, int size)
        {
            Name = name;
            Size = size;
        }
    }
}