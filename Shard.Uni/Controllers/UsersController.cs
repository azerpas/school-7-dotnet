using Microsoft.AspNetCore.Mvc;
using Shard.Uni.Services;
using Shard.Uni.Models;
using Microsoft.AspNetCore.Http;

namespace Shard.Uni.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
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
            User usr = _userService.Users.Find(User => id == User.Id);
            if (usr == null) // Adding action
            {
                _userService.Users.Add(user);
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

