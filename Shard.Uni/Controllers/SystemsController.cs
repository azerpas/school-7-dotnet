using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Shard.Uni.Models;
using Shard.Uni.Services;

namespace Shard.Uni.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SystemsController : ControllerBase
    {

        private readonly SectorService _sectorService;

        public SystemsController(SectorService sectorService)
        {
            _sectorService = sectorService;
        }

        // GET: /Systems
        [HttpGet]
        public List<StarSystem> Get()
        {
            return _sectorService.Systems;
        }

        // GET /Systems/{systemName}
        [HttpGet("{systemName}")]
        public ActionResult<StarSystem> Get(string systemName)
        {
            StarSystem system = _sectorService.Systems.Find(System => System.Name == systemName);
            if (system == null)
            {
                return NotFound();
            }
                
            return system;
        }

        // GET /Systems/{systemName}/planets
        [HttpGet("{systemName}/planets")]
        public ActionResult<List<Planet>> GetPlanets(string systemName)
        {
            StarSystem system = _sectorService.Systems.Find(System => System.Name == systemName);
            if (system == null)
            {
                return NotFound();
            }
            return system.Planets;
        }

        // GET /Systems/{systemName}/planets/{planetName}
        [HttpGet("{systemName}/planets/{planetName}")]
        public ActionResult<Planet> GetPlanet(string systemName, string planetName)
        {
            StarSystem system = _sectorService.Systems.Find(System => System.Name == systemName);
            if (system == null)
            {
                return NotFound("System not found");
            }
            Planet planet = system.Planets.Find(Planet => Planet.Name == planetName);
            if (planet == null)
            {
                return NotFound("Planet not found");
            }
            return planet;
        }
    }
}
