using Person.Data.Model;
using Person.Data.RequestDto;

namespace Person.Data.Repostry.PersonRepo
{
    public interface IPersonRepo 
    {
        Task<EPerson> create(CreatePersonDto Dto);

      //  Task<List<EPerson>> GetAllAsync(string? FilterOn = null, string? FilterQury = null, string? SortBy = null,
         //   bool IsAscending = true, int pangNumber = 1, int pangSaiz = 100);

        Task<EPerson?> GetByIdAsync(int ID);


        Task<EPerson?> UpdateAsync(int ID, UpdatePersonDto Dto);

        Task<EPerson?> DeleteAsync(int ID);



        Task<(int totalCount, List<EPerson> data)> GetAll(Dictionary<string, string[]>? filters = null, string? sortBy = null, int PageNumber = 1, int PageSaiz = 20, string? ageFilter = null);
    }
}
