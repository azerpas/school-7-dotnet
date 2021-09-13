using System;
using System.Collections.Generic;

namespace Shard.Uni.Models
{
    public class StarSystem
    {
        public string Name { get; }
        public List<Planet> Planets { get; }

        public StarSystem(string name, List<Planet> planets)
        {
            Name = name;
            Planets = planets;
        }
    }
}
