using System.ComponentModel.DataAnnotations;

namespace UserProjectTest.DataDTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string PassWord { get; set; }
        [Required]
        public string? Role { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Image { get; set; }
    }
}
