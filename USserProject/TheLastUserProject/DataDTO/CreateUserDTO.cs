using System.ComponentModel.DataAnnotations;

namespace TheLastUserProject.DataDTO
{
    public class CreateUserDTO
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string role { get; set; }
        [Required]
        public string Password { get; set; }
        public string? PhoneNumber { get; set; }

        // public IFormFile? Image { get; set; }
    }
}
