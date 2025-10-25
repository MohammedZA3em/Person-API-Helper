using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserProjectTest.Data.Model;
using UserProjectTest.DataDTO;
using UserProjectTest.Reposotry.User;

namespace UserProjectTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;

        private readonly ITokenRepo _tokenRepo;
        public LoginController(UserManager<Users> userManager, RoleManager<Roles> roleManager, ITokenRepo tokenRepo)
        {
            this._userManager = userManager;
            this._tokenRepo = tokenRepo;
        }



        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);

            if (user == null)
                return Unauthorized("Error: Email is invalid");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDTO.Password);

            if (!isPasswordValid)
                return Unauthorized("Error: Password is invalid");


            var roles = await _userManager.GetRolesAsync(user);

            var jwtToken = _tokenRepo.CreateToken(user, roles.ToList());

            return Ok(new DTOTOken { TokenJWT = jwtToken });
        }
    }
}
