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
                                Planet.Size
                            )
                        ).ToList()
                    )
                ).ToList();
        }

    }
}
