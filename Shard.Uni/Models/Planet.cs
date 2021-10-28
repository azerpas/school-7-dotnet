using System.Collections.Generic;
using System.Linq;
using Shard.Shared.Core;

namespace Shard.Uni.Models
{
    /**
     * Class reprensenting a Planet
     */
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

        public List<ResourceKind> GetGazResourcesKind() => new List<ResourceKind>
        {
            ResourceKind.Oxygen
        };

        public List<ResourceKind> GetLiquidResourcesKind() => new List<ResourceKind>
        {
            ResourceKind.Water
        };

        /**
         * Classified in order of rarity, from rarest to most common
         */
        public List<ResourceKind> GetSolidResourcesKind() => new List<ResourceKind>
        {
            ResourceKind.Titanium,
            ResourceKind.Gold,
            ResourceKind.Aluminium,
            ResourceKind.Iron,
            ResourceKind.Carbon
        };

        public ResourceKind GetResourceToMine(string type)
        {
            switch (type)
            {
                case "solid":
                    KeyValuePair<ResourceKind, int> resourceToMine = ResourceQuantity.First();
                    // Foreach quicker than LINQ
                    foreach (KeyValuePair<ResourceKind, int> resource in ResourceQuantity)
                    {
                        if (!GetSolidResourcesKind().Contains(resource.Key))
                        {
                            // First iteration, check if its already a solid resource
                            if (!GetSolidResourcesKind().Contains(resourceToMine.Key))
                                resourceToMine = resource;
                            // Most available resource
                            if (resource.Value > resourceToMine.Value)
                                resourceToMine = resource;
                            // In case of equality, rarer wins
                            if (resource.Value == resourceToMine.Value)
                                // As highest index determines most common, we keep the one with lowest index
                                resourceToMine = GetSolidResourcesKind().IndexOf(resourceToMine.Key) > GetSolidResourcesKind().IndexOf(resource.Key) ? resource : resourceToMine;
                        }
                    }
                    return resourceToMine.Key;
                case "liquid":
                    return ResourceKind.Water;
                case "gaz":
                    return ResourceKind.Oxygen;
                default:
                    throw new System.Exception("Resource type not found, available are: solid, liquid, gaz");
            }
        }

        public void Mine(ResourceKind resourceKind)
        {
            if(ResourceQuantity[resourceKind] == 0)
            {
                throw new System.Exception("No more resources are available");
            }
            else
            {
                ResourceQuantity[resourceKind]--;
            }
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