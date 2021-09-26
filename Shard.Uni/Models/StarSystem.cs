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

    public class StarSystemPlanetDetailDto
    {
        public string Name { get; set; }
        public List<PlanetDetailDto> Planets { get; set; }

        public StarSystemPlanetDetailDto(string name, List<PlanetDetailDto> planets)
        {
            Name = name;
            Planets = planets;
        }
    }
}
