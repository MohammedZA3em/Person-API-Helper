using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheLastUserProject.DataDTO;
using UserProjectTest.Data;
using UserProjectTest.Data.Model;
using UserProjectTest.DataDTO;
using static System.Net.Mime.MediaTypeNames;

namespace TheLastUserProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<Roles> _roleManager;

        public UserController(UserDbContext context, UserManager<Users> userManager, RoleManager<Roles> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // إنشاء مستخدم
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDTO user)
        {

            var User = new Users()
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(User, user.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            if (!await _roleManager.RoleExistsAsync(user.role))
            {
                return BadRequest();
            }
            await _userManager.AddToRoleAsync(User, user.role);

            var DTO = new ResponsUserDTO()
            {
                Id = User.Id,
                UserName = User.UserName,
                Email = User.Email,
                PhoneNumber = User.PhoneNumber,
                role = user.role,
                Image = User.ImagesUrl
            };

            return Ok(DTO);
        }

        // جلب كل المستخدمين
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            //var DTOs = await 
            //    (
            //    from user in _context.Users
            //    join userRole in _context.UserRoles on user.Id equals userRole.UserId
            //    join role in _context.Roles on userRole.RoleId equals role.Id
            //    select new ResponsUserDTO
            //    {
            //        Id = user.Id,
            //        UserName = user.UserName,
            //        Email = user.Email,
            //        PhoneNumber = user.PhoneNumber,
            //        role = role.Name, // اسم الدور من جدول Roles
            //        Image = user.ImagesUrl
            //    }).ToListAsync();

            var DTOs = await _context.Users
      .Join(_context.UserRoles,
            u => u.Id,
            ur => ur.UserId,
            (u, ur) => new { u, ur })
      .Join(_context.Roles,
            temp => temp.ur.RoleId,
            r => r.Id,
            (temp, r) => new ResponsUserDTO
            {
                Id = temp.u.Id,
                UserName = temp.u.UserName,
                Email = temp.u.Email,
                PhoneNumber = temp.u.PhoneNumber,
                role = r.Name,
                Image = temp.u.ImagesUrl
            })
      .ToListAsync();

            return Ok(DTOs);
        }

        // جلب مستخدم حسب Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {

            var userDTO = await (from u in _context.Users
                                 join ur in _context.UserRoles on u.Id equals ur.UserId
                                 join r in _context.Roles on ur.RoleId equals r.Id
                                 where u.Id == id
                                 select new ResponsUserDTO
                                 {
                                     Id = u.Id,
                                     UserName = u.UserName,
                                     Email = u.Email,
                                     PhoneNumber = u.PhoneNumber,
                                     role = r.Name,
                                     Image = u.ImagesUrl
                                 }).FirstOrDefaultAsync(); // نستخدم FirstOrDefault لجلب مستخدم واحد

            if (userDTO == null)
                return NotFound(); // إذا لم يوجد المستخدم

            return Ok(userDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserDTO user)
        {
            // جلب المستخدم من قاعدة البيانات مباشرة
            var existingUser = await _context.Users.FindAsync(id);

            if (existingUser == null)
                return NotFound(); // المستخدم غير موجود

            // تحديث الحقول المطلوبة
            existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.ImagesUrl = user.Image;

            // إذا أردت تحديث الدور، يمكننا عمل ذلك هنا باستخدام UserRoles و Roles
            if (!string.IsNullOrEmpty(user.Role))
            {
                // جلب الدور الحالي
                var currentRoles = await _userManager.GetRolesAsync(existingUser);

                // إزالة المستخدم من الأدوار الحالية
                await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);

                // إضافة الدور الجديد
                if (!await _roleManager.RoleExistsAsync(user.Role))
                {
                    await _roleManager.CreateAsync(new Roles { Name = user.Role });
                }
                await _userManager.AddToRoleAsync(existingUser, user.Role);
            }

            // حفظ التغييرات
            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            return Ok(existingUser);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // جلب المستخدم من قاعدة البيانات
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
                return NotFound(); // المستخدم غير موجود

            // إزالة المستخدم من أي أدوار مرتبطة به (اختياري ولكن موصى به)
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, roles);
            }

            // حذف المستخدم
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors); // حدث خطأ أثناء الحذف

            return Ok($"User with ID {id} has been deleted successfully.");
        }

    }


}
