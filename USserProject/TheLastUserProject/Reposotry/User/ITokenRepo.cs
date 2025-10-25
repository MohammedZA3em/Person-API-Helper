using UserProjectTest.Data.Model;

namespace UserProjectTest.Reposotry.User
{
    public interface ITokenRepo
    {
        string CreateToken(Users user, List<string> Roles);
    }
}
