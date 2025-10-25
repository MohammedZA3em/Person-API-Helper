using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Person.Data.Model;
using Person.Data.Repostry.UserRepo;

namespace NZWalks.API.Repostre
{
    public class TokenRepo : ITokenRepo
    {
        public readonly IConfiguration Configuration;

        public TokenRepo(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public string CreateToken(Users user, List<string> Roles)
        {
            var claims = new List<Claim>
             {
                  new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // إذا كان Id من نوع int
                  new Claim(ClaimTypes.Email, user.Email ?? ""),
                  new Claim(ClaimTypes.Name, user.UserName ?? "")
              };

            foreach (var role in Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Configuration["Jwt:Issuer"],
                audience: Configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}