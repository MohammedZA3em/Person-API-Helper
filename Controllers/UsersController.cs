using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Person.Data.Model;

namespace Person.Controllers
{
    [Route("Users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<Roles> _roleManager;

        public UsersController(UserManager<Users> userManager, RoleManager<Roles> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // CREATE: Add a new user
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] Users user, string password)
        {
            if (user == null || string.IsNullOrEmpty(password))
                return BadRequest("Invalid user data or password.");

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(user);
        }

        // READ: Get all users
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            return Ok(users);
        }

        // READ: Get user by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            return Ok(user);
        }

        // UPDATE: Update user info
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] Users updatedUser)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            user.UserName = updatedUser.UserName;
            user.Email = updatedUser.Email;
            user.ImagesUrl = updatedUser.ImagesUrl;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(user);
        }

        // DELETE: Delete user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok($"User with Id {id} deleted successfully.");
        }
    }
}
