using System.Collections.Generic;
using System.Linq;
using Shard.Shared.Core;
using Shard.Uni.Models;
using Shard.Uni.Services;
using Xunit;

namespace Shard.Uni.Tests
{
    public class SectorUnitTests
    {
        SectorService _sectorService;

        public SectorUnitTests(SectorService sectorService)
        {
            _sectorService = sectorService;
        }

        [Fact(Skip="No yet implemented")]
        public void SystemsContainResources()
        {
            List<StarSystem> systems = _sectorService.Systems;
            List<Planet> planets = systems.Select(System => System.Planets).SelectMany(Planets => Planets).ToList();

            foreach(Planet planet in planets)
            {
                foreach(ResourceKind resourceKind in planet.ResourceQuantity.Keys)
                {
                    Assert.Contains(resourceKind.ToString(), new[]
                    {
                        "Carbon",
                        "Iron",
                        "Gold",
                        "Aluminium",
                        "Titanium",
                        "Water",
                        "Oxygen",
                    });
                }
            }
        }
    }
}

