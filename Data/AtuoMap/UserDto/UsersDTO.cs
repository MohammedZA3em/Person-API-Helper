using System.ComponentModel.DataAnnotations;

namespace Person.Data.AtuoMap.UserDto
{
    public class UsersDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string UserNmae { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string RoleName { get; set; }
        public string? PhoneNumber { get; set; }

        public string? ImageUrl { get; set; }

    }
}
