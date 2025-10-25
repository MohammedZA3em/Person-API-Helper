using System;
using System.ComponentModel.DataAnnotations;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;

namespace Person.Data.RequestDto
{
    public class CreatePersonDto
    {
        [Required]
        [StringLength(20)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Last name cannot exceed 20 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "the Birth Day is Required")]
        [DataType(DataType.Date)]
        public DateTime BirthDay { get; set; }

        public IFormFile? Image { get; set; }
    }
}

