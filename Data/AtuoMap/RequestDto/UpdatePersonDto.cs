using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Person.Data.RequestDto
{
    public class UpdatePersonDto
    {
        [Required(ErrorMessage = "the First Name is Required")]
        [StringLength(20, ErrorMessage = "First name cannot exceed 20 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "the Last Name is Required")]
        [StringLength(20, ErrorMessage = "Last name cannot exceed 20 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "the Birth Day is Required")]
        [DataType(DataType.Date)]
        public DateTime BirthDay { get; set; }
        [FromForm]
        public IFormFile? Image { get; set; }
    }
}
