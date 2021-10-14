using Shard.Uni.Models;
using System.Collections.Generic;

namespace Shard.Uni.Services
{
    public class UserService
    {
        public List<User> Users { get; set; } = new();
        public Dictionary<string, List<Unit>> Units { get; set; } = new();
        public Dictionary<string, List<Building>> Buildings { get; set; } = new();

        public UserService()
        {
            Users = new List<User>();
            Units = new Dictionary<string, List<Unit>>();
            Buildings = new Dictionary<string, List<Building>>();
        }
    }
}

