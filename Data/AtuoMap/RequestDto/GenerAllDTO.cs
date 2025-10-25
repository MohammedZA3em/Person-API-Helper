using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Person.Data.RequestDto
{
    public class GenerAllDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Id { get; set; }   
        public string? Age { get; set; }  
        public string? BirthDay { get; set; } 


    }
}
