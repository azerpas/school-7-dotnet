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
        public ActionResult<UserResourcesDetailDto> Put(string id, CreateUserDto userDto)
        {
            if (id != userDto.Id)
            {
                return BadRequest();
            }

            // Format validation
            string pattern = @"(([a-f0-9]{8}-[a-f0-9]{4}-4[a-f0-9]{3}-[89aAbB][a-f0-9]{3}-[a-f0-9]{12})|(^[A-Za-z0-9]+$))";
            if (!Regex.IsMatch(userDto.Id, pattern, RegexOptions.Multiline))
            {
                return BadRequest("User Id is incorrect");
            }

            User previousUser = _userService.Users.Find(User => id == User.Id);
            User user;
            if (previousUser == null) // Adding action
            {
                user = new User(userDto.Id, userDto.Pseudo, HttpContext.User.IsInRole(Constants.Roles.Shard) ? true : false);
                // Save User
                _userService.Users.Add(user);

                // If not remote shard
                if (!HttpContext.User.IsInRole(Constants.Roles.Shard))
                {
                    // Get Random System
                    Random random = new Random();
                    int index = random.Next(_sectorService.Systems.Count);
                    StarSystem system = _sectorService.Systems[index];
                    // Get Random Planet
                    index = random.Next(system.Planets.Count);
                    Planet planet = system.Planets[index];

                    // Add default Unit
                    Unit unitScout = new Scout(system.Name, null);
                    Unit unitBuilder = new Builder(system.Name, null);
                    _userService.Units.Add(user.Id, new List<Unit> { unitScout, unitBuilder });
                    _userService.Buildings.Add(user.Id, new List<Building> { });
                }
            }
            else // Replacement action
            {
                // Use previous user to construct new one (creates a copy without ICloneable)
                user = new User(
                    userDto.Id != null ? userDto.Id : previousUser.Id, 
                    userDto.Pseudo != null ? userDto.Pseudo : previousUser.Pseudo, 
                    previousUser.DateOfCreation, 
                    previousUser.ResourcesQuantity
                );

                // If user is admin
                if (HttpContext.User.IsInRole(Constants.Roles.Admin))
                {
                    if(userDto.ResourcesQuantity != null)
                    {
                        user.ReplaceResources(userDto.ResourcesQuantity);
                        _userService.Users.Remove(previousUser);
                        _userService.Users.Add(user);
                    }
                }
                // else ignore (V5)
            }

            return new UserResourcesDetailDto(user);
        }
    }
}

