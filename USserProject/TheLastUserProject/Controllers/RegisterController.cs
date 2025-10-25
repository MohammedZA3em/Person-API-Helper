using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserProjectTest.Data.Model;
using UserProjectTest.DataDTO;

namespace UserProjectTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class RegisterController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<Roles> _roleManager;

        public RegisterController(UserManager<Users> userManager, RoleManager<Roles> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }


        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDTO userDto)
        {
            // التحقق من وجود المستخدم
            var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
            if (existingUser != null)
            {
                return BadRequest("This email is used for entertainment.");
            }

            try
            {
                // إنشاء المستخدم
                var appUser = new Users
                {
                    UserName = userDto.UserName,
                    PhoneNumber = userDto.PhoneNumber,
                    Email = userDto.Email,
                    // ImagesUrl= Image
                    EmailConfirmed = true
                };

                var createdUser = await _userManager.CreateAsync(appUser, userDto.Password);
                if (!createdUser.Succeeded)
                {
                    // return ErrorResponseHelper.ServerError("User Not Succeeded");
                }

                // التأكد من وجود الدور "User" أولاً
                var roleExists = await _roleManager.RoleExistsAsync("User");
                if (!roleExists)
                {
                    var newRole = new Roles { Name = "User", NormalizedName = "USER" };
                    await _roleManager.CreateAsync(newRole);
                }

                // إضافة المستخدم إلى الدور
                var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
                if (!roleResult.Succeeded)
                {
                    //  return ErrorResponseHelper.ServerError("Role Not Succeeded");
                }
            }
            catch (Exception ex)
            {
                // return ErrorResponseHelper.ServerError($"An error occurred during registration : {ex.Message}");
            }

            return Ok("User registration has been successful. Plz Loging");

        }

    }
}