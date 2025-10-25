using System.ComponentModel.DataAnnotations;

namespace Person.Data.AtuoMap.UserDto
{
    public class UserRigisterDto
    {
        [Required]
        public string UserNmae { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
