using System;
using Shard.Shared.Core;
using System.Collections.Generic;
using Shard.Uni.Models;
using System.Linq;

namespace Shard.Uni.Services
{
    public class SectorService
    {
        public List<StarSystem> Systems { get; set; } = new();

        public SectorService(MapGenerator generator)
        {
            var sectorSpecification = generator.Generate();
            Systems = sectorSpecification.Systems.ToList()
                .Select(                                // For each system...       (of the sector)
                    System => new StarSystem(           // Create a new object from our StarSystem constructor
                        System.Name,
                        System.Planets.Select(          // For each planet...       (of the system)
                            Planet => new Planet(       // Create a new object from our Planet constructor
                                Planet.Name,
                                Planet.Size,
                                Planet.ResourceQuantity.Select(
                                    Resource => Resource // Select the current resource
                                ).ToDictionary(
                                    Resource => Resource.Key, // Set the Key
                                    Resource => Resource.Value // Set the Value
                                )
                            )
                        ).ToList()
                    )
                ).ToList();
        }

        /**
         * This method return a StarSystem without its Planets resources
         */
        public List<StarSystemPlanetDetailDto> GetSystems()
        {
            return Systems.Select(System =>
                new StarSystemPlanetDetailDto(
                    System.Name,
                    System.Planets.Select(Planet =>
                        new PlanetDetailDto(
                            Planet.Name,
                            Planet.Size)
                        ).ToList()
                )
            ).ToList();
        }
    }
}
