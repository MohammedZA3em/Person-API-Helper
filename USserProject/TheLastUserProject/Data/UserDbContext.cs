using System;
using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserProjectTest.Data.Model;

namespace UserProjectTest.Data
{
    public class UserDbContext : IdentityDbContext<Users, Roles, int>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }
        //add dbset Models

          public DbSet<Persons> persons { get; set; }




        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // إنشاء الـ Roles
            var userRole = new Roles { Id = 3, Name = "User", NormalizedName = "USER" };
            var adminRole = new Roles { Id = 2, Name = "Admin", NormalizedName = "ADMIN" };
            var superAdminRole = new Roles { Id = 1, Name = "SuperAdmin", NormalizedName = "SUPERADMIN" };
            builder.Entity<Roles>().HasData(userRole, adminRole, superAdminRole);
            //  إنشاء المستخدمSuperAdmin
            var superAdminUser = new Users
            {
                Id = 1,
                UserName = "superadmin@system.com",
                NormalizedUserName = "SUPERADMIN@SYSTEM.COM",
                Email = "superadmin@system.com",
                NormalizedEmail = "SUPERADMIN@SYSTEM.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAEAACcQAAAAEJZ2J0e6Q1JX0k3c2z6Fq5z9Hq2P4kqZ5+g9n3Q==", // superadmin123
                SecurityStamp = "00000000-0000-0000-0000-000000000001",
                ConcurrencyStamp = "11111111-1111-1111-1111-111111111111" // اضف قيمة ثابتة
            };
            builder.Entity<Users>().HasData(superAdminUser);
            // ربط المستخدم بالدور SuperAdmin
            builder.Entity<IdentityUserRole<int>>().HasData(new IdentityUserRole<int>
            {
                RoleId = 1,
                UserId = 1
            });


        }
    }
}
