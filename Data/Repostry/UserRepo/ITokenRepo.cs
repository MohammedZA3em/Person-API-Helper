using Microsoft.AspNetCore.Identity;
using Person.Data.Model;

namespace Person.Data.Repostry.UserRepo
{
    public interface ITokenRepo
    {
        string CreateToken(Users user, List<string> Roles);
    }
}
