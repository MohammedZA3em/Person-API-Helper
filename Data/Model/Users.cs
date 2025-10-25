using Microsoft.AspNetCore.Identity;

namespace Person.Data.Model
{
    public class Users:IdentityUser<int>
    {
        public string? ImagesUrl {  get; set; }

    }
}
