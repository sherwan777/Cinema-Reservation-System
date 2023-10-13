using CinemaReservationSystemApi.Model;
using CinemaReservationSystemApi.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaReservationSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // GET: api/Users
        [HttpGet]
        public ActionResult<List<User>> Get() => _userService.Get();

        // GET: api/Users/{id}
        [HttpGet("{id}", Name = "GetUser")]
        public ActionResult<User> Get(string id)
        {
            var user = _userService.GetUserById(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // POST: api/Users
        [HttpPost]
        public ActionResult<User> Create(User user)
        {
            // Check if a user with the same ID already exists
            var existingUser = _userService.GetUserById(user.id);

            if (existingUser != null)
            {
                // If user already exists, return a conflict status code along with a message
                return Conflict(new { Message = $"User with ID: {user.id} already exists." });
            }

            // If user does not exist, create the user and return a created status code
            _userService.Create(user);
            return CreatedAtRoute("GetUser", new { id = user.id.ToString() }, user);
        }


        // DELETE: api/Users/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var user = _userService.GetUserById(id);

            if (user == null)
            {
                return NotFound();
            }

            _userService.Remove(user.id);

            return NoContent();
        }
    }

}
