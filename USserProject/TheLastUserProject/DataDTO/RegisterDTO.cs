using System.ComponentModel.DataAnnotations;

namespace UserProjectTest.DataDTO
{
    public class RegisterDTO
    {

        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string? PhoneNumber { get; set; }
        // public IFormFile? Image { get; set; }
    }
}
