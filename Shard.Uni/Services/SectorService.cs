using System;
using Shard.Shared.Core;
using System.Collections.Generic;
using Shard.Uni.Models;
using System.Linq;

namespace Shard.Uni.Services
{
    public class SectorService
    {
        public List<PlanetarySystem> Systems { get; set; } = new();

        public SectorService(MapGenerator generator)
        {
            var sectorSpecification = generator.Generate();
            Systems = sectorSpecification.Systems.ToList().Select(System => new PlanetarySystem(System.Name)).ToList();
        }

    }
}
