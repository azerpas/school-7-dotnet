using Shard.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shard.Uni.Models
{
    public class Cargo : FightingUnit
    {
        public Dictionary<ResourceKind, int> ResourceQuantity { get; }

        public Cargo(string system, string? planet, Dictionary<string, int>? resourceQuantity, int? health = null) 
            : base("cargo", health ?? Constants.Fighters.Health.Cargo, Constants.Fighters.Damage.Cargo, Constants.Fighters.Timeout.Cargo, system, planet) 
        {
            ResourceQuantity = resourceQuantity != null ? resourceQuantity.ToDictionary(
                resource => (ResourceKind)Enum.Parse(typeof(ResourceKind), Utils.Strings.Capitalize(resource.Key)), 
                resource => resource.Value
            ) : new Dictionary<ResourceKind, int> { };
        }
        public Cargo(string id, string system, string? planet, Dictionary<string, int>? resourceQuantity, int? health) 
            : base(id, "cargo", health ?? Constants.Fighters.Health.Cargo, Constants.Fighters.Damage.Cargo, Constants.Fighters.Timeout.Cargo, system, planet) 
        {
            ResourceQuantity = resourceQuantity != null ? resourceQuantity.ToDictionary(
                resource => (ResourceKind)Enum.Parse(typeof(ResourceKind), Utils.Strings.Capitalize(resource.Key)), 
                resource => resource.Value
            ) : new Dictionary<ResourceKind, int> { };
        }

        public Dictionary<ResourceKind, int> ResourcesToLoadUnload(Dictionary<ResourceKind, int> newResources)
        {
            Dictionary<ResourceKind, int> resourcesToLoadUnload = new Dictionary<ResourceKind, int>();
            foreach (var resource in newResources)
            {
                try
                {
                    if (ResourceQuantity[resource.Key] > 0)
                    {
                        int difference = resource.Value - ResourceQuantity[resource.Key];
                        resourcesToLoadUnload[resource.Key] = difference;
                    }
                    else
                    {
                        resourcesToLoadUnload[resource.Key] = resource.Value;
                    }
                }
                catch (KeyNotFoundException)
                {
                    resourcesToLoadUnload[resource.Key] = resource.Value;
                }
            }
            return resourcesToLoadUnload;
        }
    }
}
