using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Person.Data.Model;
using System.IO;
using Person.Data.RequestDto;
using Serilog;

namespace Person.Data.Repostry.PersonRepo
{
    public class PersonsRepo : IPersonRepo
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PersonDbContext _context;
        private readonly IWebHostEnvironment _env;



        public PersonsRepo(PersonDbContext context, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            this._context = context;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<EPerson> create(CreatePersonDto Dto)
        {
            string imageUrl = await HandlingImage(Dto.Image);

            var person = new EPerson
            {
                FirstName = Dto.FirstName,
                LastName = Dto.LastName,
                Image = imageUrl,
                BirthDay = Dto.BirthDay
            };
            try
            {
                await _context.Person.AddAsync(person);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message.ToString());
            }
            return person;

        }
        public async Task<EPerson?> GetByIdAsync(int ID)
        {
            try
            {
                var Person = await _context.Person.FindAsync(ID);
                return Person;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message.ToString());
            }
            return null;
        }
        public async Task<EPerson?> UpdateAsync(int ID, UpdatePersonDto Dto)
        {
            try
            {
                var person = await _context.Person.FindAsync(ID);
                if (person == null)
                {
                    return null;
                }
                string ImageURL = await HandlingImage(Dto.Image);
                if (!(person.Image == null || person.Image.Length == 0))
                {
                    DeleteImage(person.Image);
                }
                person.Id = ID;
                person.FirstName = Dto.FirstName;
                person.LastName = Dto.LastName;
                person.BirthDay = Dto.BirthDay;
                person.Image = ImageURL;
                await _context.SaveChangesAsync();
                return person;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message.ToString());
            }
            return null;

        }
        public async Task<EPerson?> DeleteAsync(int ID)
        {
            try
            {
                var Person = await _context.Person.FindAsync(ID);
                if (Person == null)
                {
                    return null;
                }
                DeleteImage(Person.Image);
                _context.Person.Remove(Person);
                await _context.SaveChangesAsync();

                return Person;
            }
            catch (Exception ex)
            {
               Log.Fatal( ex.Message.ToString());
            }
            return null;
        }
        public async Task<(int totalCount, List<EPerson> data)> GetAll(
         Dictionary<string, string[]>? filters = null,
         string? sortBy = null,
         int PageNumber = 1,
         int PageSize = 20,
         string? ageFilter = null)
        {


            var query = _context.Person.AsQueryable();


            // Apply dynamic filters
            if (filters != null)
            {
                foreach (var kv in filters)
                {
                    var key = kv.Key;
                    var values = kv.Value;

                    if (string.Equals(key, "Name", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var val in values)
                        {
                            var trimmed = val.Trim();
                            query = query.Where(p =>
                                EF.Functions.Like(p.FirstName, $"%{trimmed}%") ||
                                EF.Functions.Like(p.LastName, $"%{trimmed}%"));
                        }
                        continue;
                    }

                    var property = typeof(EPerson).GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (property == null) continue;

                    foreach (var rawValue in values)
                    {
                        if (string.IsNullOrWhiteSpace(rawValue)) continue;
                        var rv = rawValue.Trim();
                        string[] ops = { ">=", "<=", "!=", ">", "<", "=" };
                        string? op = ops.FirstOrDefault(o => rv.StartsWith(o));

                        object converted;
                        try
                        {
                            converted = Convert.ChangeType(rv.Substring(op?.Length ?? 0), property.PropertyType);
                        }
                        catch
                        {
                            continue;
                        }

                        if (op != null)
                        {
                            var param = Expression.Parameter(typeof(EPerson), "p");
                            var propAccess = Expression.Property(param, property);
                            var constant = Expression.Constant(converted);
                            Expression comparison = op switch
                            {
                                ">" => Expression.GreaterThan(propAccess, constant),
                                "<" => Expression.LessThan(propAccess, constant),
                                ">=" => Expression.GreaterThanOrEqual(propAccess, constant),
                                "<=" => Expression.LessThanOrEqual(propAccess, constant),
                                "!=" => Expression.NotEqual(propAccess, constant),
                                "=" => Expression.Equal(propAccess, constant),
                                _ => null
                            };
                            if (comparison != null)
                            {
                                var lambda = Expression.Lambda<Func<EPerson, bool>>(comparison, param);
                                query = query.Where(lambda);
                            }
                        }
                        else if (property.PropertyType == typeof(string))
                        {
                            query = query.Where(p => EF.Property<string>(p, property.Name).Contains(rv));
                        }
                        else
                        {
                            query = query.Where(p => EF.Property<object>(p, property.Name).Equals(converted));
                        }
                    }
                }
            }

            // Age filter as database query
            if (!string.IsNullOrWhiteSpace(ageFilter))
            {
                string[] ops = { ">=", "<=", "!=", ">", "<", "=" };
                string? op = ops.FirstOrDefault(o => ageFilter.StartsWith(o));
                if (op != null)
                {
                    int ageValue = int.Parse(ageFilter.Substring(op.Length));

                    var today = DateTime.Today;
                    Expression<Func<EPerson, bool>> agePredicate = op switch
                    {
                        ">" => p => (today.Year - p.BirthDay.Year - (p.BirthDay > today.AddYears(-(today.Year - p.BirthDay.Year)) ? 1 : 0)) > ageValue,
                        "<" => p => (today.Year - p.BirthDay.Year - (p.BirthDay > today.AddYears(-(today.Year - p.BirthDay.Year)) ? 1 : 0)) < ageValue,
                        ">=" => p => (today.Year - p.BirthDay.Year - (p.BirthDay > today.AddYears(-(today.Year - p.BirthDay.Year)) ? 1 : 0)) >= ageValue,
                        "<=" => p => (today.Year - p.BirthDay.Year - (p.BirthDay > today.AddYears(-(today.Year - p.BirthDay.Year)) ? 1 : 0)) <= ageValue,
                        "=" => p => (today.Year - p.BirthDay.Year - (p.BirthDay > today.AddYears(-(today.Year - p.BirthDay.Year)) ? 1 : 0)) == ageValue,
                        "!=" => p => (today.Year - p.BirthDay.Year - (p.BirthDay > today.AddYears(-(today.Year - p.BirthDay.Year)) ? 1 : 0)) != ageValue,
                        _ => null
                    };

                    if (agePredicate != null)
                        query = query.Where(agePredicate);
                }
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                bool desc = sortBy.StartsWith("-");
                if (desc) sortBy = sortBy[1..];

                var property = typeof(EPerson).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    query = desc
                        ? query.OrderByDescending(p => EF.Property<object>(p, property.Name))
                        : query.OrderBy(p => EF.Property<object>(p, property.Name));
                }
            }
            var Count = query.Count();

            // Paging
            int skip = (PageNumber - 1) * PageSize;
           var Result= await query.Skip(skip).Take(PageSize).ToListAsync();
            return (Count, Result);
        }

        // For Images
        public void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            try
            {
                // استخراج اسم الملف من الـ URL
                var fileName = Path.GetFileName(imageUrl);

                // بناء المسار داخل مجلد images
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "images", fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message.ToString());
                Console.WriteLine($"Error deleting image: {ex.Message}");
            }
        }
        private async Task<string> HandlingImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            // تأكد من وجود الـ HttpContext
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                throw new InvalidOperationException("HttpContext غير متوفر");

            // استخدام WebRootPath إذا موجود، وإلا ContentRootPath
            var rootPath = _env.WebRootPath ?? _env.ContentRootPath;
            if (string.IsNullOrEmpty(rootPath))
                throw new InvalidOperationException("مسار الجذر غير معرف");

            // إنشاء مجلد الصور إذا لم يكن موجودًا
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // توليد اسم فريد للملف
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // حفظ الملف على السيرفر
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // بناء URL للوصول للصورة
            var imageUrl = $"{request.Scheme}://{request.Host}/images/{fileName}";
            return imageUrl;

        }

    }
}




