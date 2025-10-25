using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Person.Data.AtuoMap.UserDto;
using Person.Data.Model;

namespace Person.Data
{
    public class PersonDbContext : IdentityDbContext<Users, Roles, int>
    {
        public PersonDbContext(DbContextOptions<PersonDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        //add Entity here
        public DbSet<EPerson> Person {  get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // إنشاء الـ Roles
            var userRole = new Roles { Id = 3, Name = "User", NormalizedName = "USER" };
            var adminRole = new Roles { Id = 2, Name = "Admin", NormalizedName = "ADMIN" };
            var superAdminRole = new Roles { Id = 1, Name = "SuperAdmin", NormalizedName = "SUPERADMIN" };

            builder.Entity<Roles>().HasData(userRole, adminRole, superAdminRole);

            // إنشاء المستخدم SuperAdmin
            var hasher = new PasswordHasher<Users>();
            var superAdminUser = new Users
            {
                Id = 1,
                UserName = "SuperAdmin",
                NormalizedUserName = "SUPERADMIN",
                Email = "SuperAdmin@gmail.com",
                NormalizedEmail = "SUPERADMIN@GMAIL.COM",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "SuperAdmin123")
            };

            builder.Entity<Users>().HasData(superAdminUser);

            // ربط المستخدم بالدور SuperAdmin
            builder.Entity<IdentityUserRole<int>>().HasData(new IdentityUserRole<int>
            {
                RoleId = 1, // SuperAdmin
                UserId = 1  // SuperAdmin user
            });
        }
    }
}


