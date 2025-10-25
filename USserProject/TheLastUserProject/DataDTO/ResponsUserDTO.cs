using System.ComponentModel.DataAnnotations;

namespace TheLastUserProject.DataDTO
{
    public class ResponsUserDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string role { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Image { get; set; }
    }
}
