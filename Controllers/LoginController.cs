using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Person.Data.AtuoMap.UserDto;
using Person.Data.Model;
using Person.Data.Repostry.UserRepo;

namespace Person.Controllers
    
{
    [Route("Login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<Roles> _roleManager;

        private readonly ITokenRepo _tokenRepo;

        public LoginController(UserManager<Users> userManager, RoleManager<Roles> roleManager, ITokenRepo tokenRepo)
        {
            this._userManager = userManager;
            this._tokenRepo = tokenRepo;
            this._roleManager = roleManager;

        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);

            if (user == null)
                return Unauthorized("Error: Username is invalid");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDTO.Password);

            if (!isPasswordValid)
                return Unauthorized("Error: Password is invalid");

            var roles = await _userManager.GetRolesAsync(user);
            var jwtToken = _tokenRepo.CreateToken(user, roles.ToList());

            return Ok(new DTOTOken { Token = jwtToken });
        }

    }
}
