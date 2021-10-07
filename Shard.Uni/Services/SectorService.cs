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
                .Select(
                    System => new StarSystem(
                        System.Name,
                        System.Planets.Select(
                            Planet => new Planet(
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
