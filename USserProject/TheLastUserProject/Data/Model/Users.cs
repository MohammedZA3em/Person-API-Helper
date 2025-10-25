using Microsoft.AspNetCore.Identity;

namespace UserProjectTest.Data.Model
{
    public class Users : IdentityUser<int>
    {
        public string? ImagesUrl { get; set; }

    }
}
