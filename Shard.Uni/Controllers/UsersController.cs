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
        public ActionResult<UserResourcesDetailDto> Get(string id)
        {
            User user = _userService.Users.Find(User => id == User.Id);
            if (user == null)
            {
                return NotFound();
            }

            return new UserResourcesDetailDto(user);
        }

        // PUT /Users/{id}
        [HttpPut("{id}")]
        public ActionResult<User> Put(string id, CreateUserDto userDto)
        {
            if (id != userDto.Id)
            {
                return BadRequest();
            }
            string pattern = @"(([a-f0-9]{8}-[a-f0-9]{4}-4[a-f0-9]{3}-[89aAbB][a-f0-9]{3}-[a-f0-9]{12})|(^[A-Za-z0-9]+$))";
            if (!Regex.IsMatch(userDto.Id, pattern, RegexOptions.Multiline))
            {
                return BadRequest("User Id is incorrect");
            }

            User usr = _userService.Users.Find(User => id == User.Id);
            User user = new User(userDto.Id, userDto.Pseudo);
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
                Unit unitScout = new Unit("scout", system.Name, null);
                Unit unitBuilder = new Unit("builder", system.Name, null);
                _userService.Units.Add(user.Id, new List<Unit> { unitScout, unitBuilder });
                _userService.Buildings.Add(user.Id, new List<Building> { });
            }
            else // Replacement action
            {
                if (HttpContext.User.IsInRole(Constants.Roles.Admin))
                {
                    if(userDto.ResourcesQuantity != null)
                    {
                        user.ReplaceResources(userDto.ResourcesQuantity);
                        _userService.Users.Remove(usr);
                        _userService.Users.Add(user);
                    }
                }
                else
                {
                    // Sinon, ignoré ? TODO:
                    // _userService.Users.Remove(usr);
                    // _userService.Users.Add(user);
                }
            }

            return user;
        }
    }
}

