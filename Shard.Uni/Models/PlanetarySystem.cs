using System;
using System.Collections.Generic;

namespace Shard.Uni.Models
{
    public class PlanetarySystem
    {
        public string Name { get; }
        public List<Planet> Planets { get; }

        public PlanetarySystem(string name)
        {
            Name = name;
        }
    }
}
