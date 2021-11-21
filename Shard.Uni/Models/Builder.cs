using System;
namespace Shard.Uni.Models
{
    public class Builder : Unit
    {
        public Builder(string system, string? planet) : base("builder", system, planet) { }
        public Builder(string id, string system, string? planet) : base(id, "builder", system, planet) { }
    }
}

