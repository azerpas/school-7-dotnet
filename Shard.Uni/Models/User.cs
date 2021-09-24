using System;
namespace Shard.Uni.Models
{
    public class User
    {
        private string id { get; }
        private string pseudo { get; }
        private string dateOfCreation { get; }
        public User(string id, string pseudo)
        {
            this.id = id;
            this.pseudo = pseudo;
            this.dateOfCreation = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:sssK");
        }
    }
}

