using Microsoft.AspNetCore.Mvc;
using Shard.Uni.Services;
using Shard.Uni.Models;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace Shard.Uni.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly SectorService _sectorService;

        public UsersController(UserService userService, SectorService sectorService)
        {
            _userService = userService;
            _sectorService = sectorService;
        }

        // GET /Users/{id}
        [HttpGet("{id}")]
        public ActionResult<User> Get(string id)
        {
            User user = _userService.Users.Find(User => id == User.Id);
            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT /Users/{id}
        [HttpPut("{id}")]
        public ActionResult<User> Put(string id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }
            string pattern = @"(([a-f0-9]{8}-[a-f0-9]{4}-4[a-f0-9]{3}-[89aAbB][a-f0-9]{3}-[a-f0-9]{12})|(^[A-Za-z0-9]+$))";
            if (!Regex.IsMatch(user.Id, pattern, RegexOptions.Multiline))
            {
                return BadRequest("User Id is incorrect");
            }

            User usr = _userService.Users.Find(User => id == User.Id);
            if (usr == null) // Adding action
            {
                // Save User
                _userService.Users.Add(user);

                // Get Random System
                Random random = new Random();
                int index = random.Next(_sectorService.Systems.Count);
                StarSystem system = _sectorService.Systems[index];
                // Get Random Planet
                index = random.Next(system.Planets.Count);
                Planet planet = system.Planets[index];

                // Add default Unit
                Unit unit = new Unit("scout", system.Name, planet.Name);
                _userService.Units.Add(user.Id, new List<Unit> { unit });
            }
            else // Replacement action
            {
                _userService.Users.Remove(usr);
                _userService.Users.Add(user);
            }

            return user;
        }
    }
}

