using System.ComponentModel.DataAnnotations.Schema;

namespace Person.Data.RequestDto
{
    public class FindPersonDto
    {
        public int Id { get; }
       public string FullName { get; }=string.Empty;
    }
}
