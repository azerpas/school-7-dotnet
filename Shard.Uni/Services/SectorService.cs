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
                                Planet.Size
                            )
                        ).ToList()
                    )
                ).ToList();
        }

    }
}
