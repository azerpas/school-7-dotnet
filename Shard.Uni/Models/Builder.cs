using System;
namespace Shard.Uni.Models
{
    public class Builder : Unit
    {
        public Builder(string system, string? planet) : base(system, planet) { }
        public Builder(string id, string system, string? planet) : base(id, system, planet) { }
    }
}

