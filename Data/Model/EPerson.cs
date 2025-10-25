using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Person.Data.Model
{
    public class EPerson
    {
        public int Id { get; set; }
        [Required (ErrorMessage = "the First Name is Required")]
        [StringLength(20,ErrorMessage = "First name cannot exceed 20 characters.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "the Last Name is Required")]
        [StringLength(20, ErrorMessage = "Last name cannot exceed 20 characters.")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "the Birth Day is Required")]
        [DataType(DataType.Date)]
        public DateTime BirthDay { get; set; }

        [NotMapped]
        public string FullName { get { return FirstName + LastName; } }

        [NotMapped] // مهم جداً: لا يُخزن في قاعدة البيانات
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

        // يمكنك حفظ رابط الصورة أو مسارها المحلي
        public string? Image {  get; set; }

    }
}
