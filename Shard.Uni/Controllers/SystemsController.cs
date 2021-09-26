using System.Collections.Generic;
using System.Linq;
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
        public List<StarSystemPlanetDetailDto> Get()
        {
            return _sectorService.GetSystems();
        }

        // GET /Systems/{systemName}
        [HttpGet("{systemName}")]
        public ActionResult<StarSystemPlanetDetailDto> Get(string systemName)
        {
            StarSystemPlanetDetailDto system = _sectorService.GetSystems().Find(System => System.Name == systemName);
            if (system == null)
            {
                return NotFound();
            }
                
            return system;
        }

        // GET /Systems/{systemName}/planets
        [HttpGet("{systemName}/planets")]
        public ActionResult<List<PlanetDetailDto>> GetPlanets(string systemName)
        {
            StarSystemPlanetDetailDto system = _sectorService.GetSystems().Find(System => System.Name == systemName);
            if (system == null)
            {
                return NotFound();
            }
            return system.Planets;
        }

        // GET /Systems/{systemName}/planets/{planetName}
        [HttpGet("{systemName}/planets/{planetName}")]
        public ActionResult<PlanetDetailDto> GetPlanet(string systemName, string planetName)
        {
            StarSystemPlanetDetailDto system = _sectorService.GetSystems().Find(System => System.Name == systemName);
            if (system == null)
            {
                return NotFound("System not found");
            }
            PlanetDetailDto planet = system.Planets.Find(Planet => Planet.Name == planetName);
            if (planet == null)
            {
                return NotFound("Planet not found");
            }
            return planet;
        }

        /*
         * ----------
         * GetPlanet: en une seule requête
         * ---------- 
        // GET /Systems/{systemName}/planets?name={planetName}
        [HttpGet("{systemName}")]
        public ActionResult<Planet> GetPlanet([FromUri] string systemName, [FromQuery] string planetName)
        {
            StarSystem system = _sectorService.Systems.Find(System => System.Name == systemName);
            if (system == null)
            {
                return NotFound("System not found");
            }
            if(planetName != null)
            {
                Planet planet = system.Planets.Find(Planet => Planet.Name == planetName);
                if (planet == null)
                {
                    return NotFound("Planet not found");
                }
                return planet;
            }
            
        }
        */
    }
}
