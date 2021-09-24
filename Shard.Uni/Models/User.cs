using System;
namespace Shard.Uni.Models
{
    public class User
    {
        public string Id { get; }
        public string Pseudo { get; }
        public string DateOfCreation { get; }

        public User(string id, string pseudo)
        {
            Id = id;
            Pseudo = pseudo;
            DateOfCreation = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:sssK");
        }
    }
}

