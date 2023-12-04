using CinemaReservationSystemApi.Model;
using CinemaReservationSystemApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace CinemaReservationSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<MoviesController> _logger;
        private readonly string secretKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKaWduZXNoIFRyaXZlZGkiLCJlbWFpbCI6InRlc3QuYnRlc3RAZ21haWwuY29tIiwiRGF0ZU9mSm9pbmciOiIwMDAxLTAxLTAxIiwianRpIjoiYzJkNTZjNzQtZTc3Yy00ZmUxLTgyYzAtMzlhYjhmNzFmYzUzIiwiZXhwIjoxNTMyMzU2NjY5LCJpc3MiOiJUZXN0LmNvbSIsImF1ZCI6IlRlc3QuY29tIn0.8hwQ3H9V8mdNYrFZSjbCpWSyR1CNyDYHcGf6GqqCGnY";
       
        public UsersController(UserService userService , ILogger<MoviesController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: api/Users
        [HttpGet]
        public ActionResult<List<User>> Get() => _userService.Get();

        // GET: api/Users/{id}
        [HttpGet("{id:length(24)}", Name = "GetUser")]
        public ActionResult<User> GetUserbyId(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid ID format");
            }

            var user = _userService.GetUserById(objectId);

            if (user == null)
            {
                return NotFound(new { Message = $"User with ID: {id} not found" });
            }

            return user;
        }

        [HttpPost("login")]
        public ActionResult<User> Login([FromBody] LoginRequest request)
        {
            var user = _userService.GetUserByEmailAndPassword(request.email, request.password);

            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                    new Claim(ClaimTypes.Name , user.name),
                    new Claim(ClaimTypes.Email , user.email),
                    new Claim(ClaimTypes.Role, user.isAdmin ? "admin" : "user")
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Set the token in the cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Path = "/"
            };
            // HttpContext.Response.Cookies.Append("token", tokenString, cookieOptions);

            return Ok(new
            {
                Message = "Login successful",
                Token = tokenString,
                Name = user.name,
                Email = user.email,
                Role = user.isAdmin ? "admin" : "user",
                UserId = user.id.ToString(),
            });
        }

        // POST: api/Users
        [HttpPost]
        public ActionResult<object> Create(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = _userService.GetUserByEmail(user.email);

            if (existingUser != null)
            {
                return Conflict(new { Message = $"User with Email: {user.email} already exists." });
            }

            var createdUser = _userService.Create(user);

            // Generate JWT token for the new user
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, createdUser.id.ToString()),
                    new Claim(ClaimTypes.Name , createdUser.name),
                    new Claim(ClaimTypes.Email , user.email),
                    new Claim(ClaimTypes.Role, createdUser.isAdmin ? "admin" : "user")
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Save token to cookies
            //Response.Cookies.Append("jwt", tokenString, new CookieOptions { HttpOnly = true, Expires = DateTime.UtcNow.AddDays(1) });

            // Return the user ID, role, and success message
            return CreatedAtRoute("GetUser", new { id = createdUser.id.ToString() }, new
            {
                UserId = createdUser.id.ToString(),
                Name = createdUser.name,
                Email = createdUser.email,
                Role = createdUser.isAdmin ? "admin" : "user",
                Message = "Registered successfully"
            });
        }

        // GET: api/Users/IsLoggedIn
        [HttpGet("IsLoggedIn")]
        public ActionResult IsLoggedIn()
        {
            var token = HttpContext.Request.Cookies["token"];
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Access attempt without a token.");
                return Unauthorized(new { Message = "No token provided. Please log in." });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var id = jwtToken.Claims.First(x => x.Type == "nameid").Value;
                var name = jwtToken.Claims.First(x => x.Type == "unique_name").Value;
                var email = jwtToken.Claims.First(x => x.Type == "email").Value;
                var role = jwtToken.Claims.First(x => x.Type == "role").Value;

                _logger.LogInformation("Token validated successfully for user: {Email}", email);

                return Ok(new
                {
                    Message = "User is logged in.",
                    Id = id,
                    Name = name,
                    Email = email,
                    Role = role
                });
            }
            catch (SecurityTokenException ste)
            {
                _logger.LogWarning(ste, "Security token exception occurred while validating the token.");
                return Unauthorized(new { Message = "Invalid token. Please log in again." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while validating the token.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred. Please try again later." });
            }
        }


        // DELETE: api/Users/{id}
        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid ID format");
            }

            var user = _userService.GetUserById(objectId);

            if (user == null)
            {
                return NotFound(new { Message = $"User with ID: {id} not found" });
            }

            _userService.Remove(objectId);
            return NoContent();
        }

    }

}
