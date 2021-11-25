using Shard.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shard.Uni.Models
{
    public class Cargo : Unit
    {
        public Dictionary<ResourceKind, int> ResourceQuantity { get; }

        public Cargo(string system, string? planet, Dictionary<string, int>? resourceQuantity) 
            : base("cargo", system, planet) 
        {
            ResourceQuantity = resourceQuantity != null ? resourceQuantity.ToDictionary(
                resource => (ResourceKind)Enum.Parse(typeof(ResourceKind), Utils.Strings.Capitalize(resource.Key)), 
                resource => resource.Value
            ) : new Dictionary<ResourceKind, int> { };
        }
        public Cargo(string id, string system, string? planet, Dictionary<string, int>? resourceQuantity) 
            : base(id, "cargo", system, planet) 
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
                if(ResourceQuantity[resource.Key] > 0)
                {
                    int difference = resource.Value - ResourceQuantity[resource.Key];
                    resourcesToLoadUnload[resource.Key] = difference;
                }
                else
                {
                    resourcesToLoadUnload[resource.Key] = resource.Value;
                }
            }
            return resourcesToLoadUnload;
        }
    }
}
