using System;
using System.Collections.Generic;
using System.Linq;
using Shard.Shared.Core;

namespace Shard.Uni.Models
{
    public class User
    {
        public string Id { get; }
        public string Pseudo { get; }
        public string DateOfCreation { get; }
        public Dictionary<ResourceKind, int> ResourcesQuantity { get; }

        public User(string id, string pseudo, bool? shard = false)
        {
            Id = id;
            Pseudo = pseudo;
            DateOfCreation = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:sssK");
            if (shard != null && shard != false)
            {
                ResourcesQuantity = new Dictionary<ResourceKind, int> {
                    { ResourceKind.Gold, 0 },
                    { ResourceKind.Aluminium, 0 },
                    { ResourceKind.Carbon, 20 },
                    { ResourceKind.Iron, 10 },
                    { ResourceKind.Oxygen, 50 },
                    { ResourceKind.Titanium, 0 },
                    { ResourceKind.Water, 50 },
                };
            }
        }

        public User(string id, string pseudo, string dateOfCreation, Dictionary<ResourceKind, int> resources)
        {
            Id = id;
            Pseudo = pseudo;
            DateOfCreation = dateOfCreation;
            ResourcesQuantity = resources;
        }

        /// <summary>
        /// Check if the current user has enough resources
        /// </summary>
        /// <param name="resources"></param>
        /// <returns></returns>
        public bool HasEnoughResources(Dictionary<ResourceKind, int> resources)
        {
            return resources.All(
                resource => resource.Value <= ResourcesQuantity[resource.Key]
            );
        }

        /// <summary>
        /// Remove the given resources quantities from the current user
        /// </summary>
        /// <param name="resources"></param>
        public void ConsumeResources(Dictionary<ResourceKind, int> resources)
        {
            foreach (KeyValuePair<ResourceKind, int> resource in resources)
            {
                ResourcesQuantity[resource.Key] = ResourcesQuantity[resource.Key] - resource.Value;
            }
        }

        /// <summary>
        /// Replace current resources with given
        /// Method used on Admin request
        /// </summary>
        /// <param name="resources"></param>
        public void ReplaceResources(Dictionary<string, int> resources)
        {
            foreach (KeyValuePair<string, int> resource in resources)
            {
                ResourceKind kind = (ResourceKind)Enum.Parse(typeof(ResourceKind), Utils.Strings.Capitalize(resource.Key));
                ResourcesQuantity[kind] = resource.Value;
            }
        }
    }

    /// <summary>
    /// A Data Transfer Object class used to fetch data from request Body
    /// </summary>
    public class CreateUserDto
    {
        public string Id { get; }
        public string Pseudo { get; }
        public Dictionary<string, int>? ResourcesQuantity { get; }

        public CreateUserDto(string id, string pseudo, Dictionary<string, int>? resourcesQuantity)
        {
            Id = id;
            Pseudo = pseudo;
            ResourcesQuantity = resourcesQuantity;
        }
    }

    /// <summary>
    /// A class that mimics the User class with the only difference that the resources (resourcesQuantity)
    /// will be returned with their Key values as lowercase 
    /// </summary>
    public class UserResourcesDetailDto
    {
        public string Id { get; }
        public string Pseudo { get; }
        public string DateOfCreation { get; }
        public Dictionary<string, int> resourcesQuantity { get; }

        public UserResourcesDetailDto(User user)
        {
            Id = user.Id;
            Pseudo = user.Pseudo;
            DateOfCreation = user.DateOfCreation;
            resourcesQuantity = user.ResourcesQuantity.ToDictionary(Resource => Resource.Key.ToString().ToLower(), Resource => Resource.Value);
        }
    }
}

