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

        public User(string id, string pseudo)
        {
            Id = id;
            Pseudo = pseudo;
            DateOfCreation = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:sssK");
            ResourcesQuantity = new Dictionary<ResourceKind, int> {
                { ResourceKind.Gold, 2 }
            };
        }
    }

    public class CreateUserDto
    {
        public string Id { get; }
        public string Pseudo { get; }

        public CreateUserDto(string id, string pseudo)
        {
            Id = id;
            Pseudo = pseudo;
        }
    }

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

