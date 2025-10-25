using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Person.Data.ResponseDto
{
    public class ResponsPersonDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDay { get; set; }
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDay.Year;
                if (BirthDay.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
        public string? Image { get; set; }
    }
}
