using System;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Person.Data.Repostry.PersonRepo;
using Person.Data.RequestDto;
using Person.Data.ResponseDto;
using Serilog;




namespace Person.Controllers
{
    [Route("Persons")]
    [ApiController]
    public class PersonsController : ControllerBase
    {

        private readonly IWebHostEnvironment _env;
        private readonly IPersonRepo _personRepo;
        private readonly IMapper _mapper;

        public PersonsController(IPersonRepo RepoPerson, IWebHostEnvironment env, IMapper mapper)
        {
            this._personRepo = RepoPerson;
            _env = env;
            this._mapper = mapper;
        }

        [Authorize]
        [HttpGet("{ID}")]
        public async Task<IActionResult> GetPersons(int ID)
        {

            if (ID <= 0)
            {
                return ErrorResponseHelper.BadRequest("ID must be > 0 ");
            }
            // here we take a data from Repo not from Sql server
            var Person = await _personRepo.GetByIdAsync (ID);
            if (Person == null)
            {
                return ErrorResponseHelper.NotFound($"Person with ID {ID} Not Found");
            }
            return Ok(_mapper.Map<ResponsPersonDTO>(Person));
        }

        [HttpPatch]
        public async Task<IActionResult> CreatePerosn([FromForm] CreatePersonDto Dto)
        {

            if (Dto == null)
            {
                return ErrorResponseHelper.BadRequest("Invalid Data");
            }
            var Person = await _personRepo.create(Dto);
            var PerosnDto = _mapper.Map<ResponsPersonDTO>(Person);
            return CreatedAtAction(nameof(GetPersons), new { id = PerosnDto.Id }, PerosnDto);
        }

        [HttpDelete("{ID}")]
        public async Task<IActionResult> DeleteByID([FromRoute] int ID)
        {

            if (ID == null || ID <= 0)
            {
                return BadRequest("Invalid Data");
            }
            var Person = await _personRepo.DeleteAsync(ID);
            if (Person == null)
            {
                return NotFound($"Not Found Person with Id {ID}");
            }
            return Ok(_mapper.Map<ResponsPersonDTO>(Person));
        }

        [HttpPut("{ID}")]
        public async Task<IActionResult> UpdatePerson([FromRoute] int ID, [FromForm] UpdatePersonDto dto)
        {
            if (ID == null )
                return BadRequest("Invalid Data");
            if(ID<=0)
                return BadRequest("must be (ID > 0)");
            if (dto == null)
                return BadRequest("Invalid Data");

            var person = await _personRepo.UpdateAsync(ID, dto);
            if (person == null)
                return NotFound($"Not Found Person with Id {ID}");

            return Ok(_mapper.Map<ResponsPersonDTO>(person));
        }

        // Filter=Name=Moah...
        //[HttpGet]
        //public async Task<IActionResult> GetAll(
        //             string? filter,
        //                      string? Name,
        //                  string? FirstName,
        //                                string? LastName,
        //                                          string? sortBy,
        //                                          int PageNumber = 1,
        //                                               int PageSize = 20)
        //{
        //    var dict = new Dictionary<string, string[]>();

        //    // 1) إذا وصل filter العام نحليله
        //    if (!string.IsNullOrWhiteSpace(filter))
        //    {
        //        var filters = filter.Split(',', StringSplitOptions.RemoveEmptyEntries);
        //        foreach (var f in filters)
        //        {
        //            // مثال: f = "Id>2"
        //            var ops = new[] { ">=", "<=", "!=", ">", "<", "=" };
        //            var op = ops.FirstOrDefault(o => f.Contains(o));

        //            if (op != null)
        //            {
        //                var parts = f.Split(op, 2);
        //                if (parts.Length == 2)
        //                {
        //                    var key = parts[0].Trim();
        //                    var value = $"{op}{parts[1].Trim()}"; // >2
        //                    dict[key] = new[] { value };
        //                }
        //            }
        //        }
        //    }

        //    // 2) حجوزات منفصلة إذا استخدمت query params مباشرة
        //    if (!string.IsNullOrWhiteSpace(Name))
        //        dict["Name"] = new[] { Name.Trim() };

        //    if (!string.IsNullOrWhiteSpace(FirstName))
        //        dict["FirstName"] = new[] { FirstName.Trim() };

        //    if (!string.IsNullOrWhiteSpace(LastName))
        //        dict["LastName"] = new[] { LastName.Trim() };

        //    // Debug: تأكد أي فلاتر استقبلنا
        //    // Console.WriteLine("Filters: " + string.Join(" | ", dict.Select(kv => $"{kv.Key}:{string.Join(";",kv.Value)}")));

        //    var result = await _personRepo.GetAll(dict, sortBy, PageNumber, PageSize);
        //    //if (result == null || result.Count == 0)
        //    //    return NotFound("No persons match the query.");
        //    return Ok(result);
        //}

        //....................................

        [HttpGet]
        public async Task<IActionResult> GetAll(
        [FromQuery] GenerAllDTO filter,
        [FromQuery] string? sortBy,
        [FromQuery] int PageNumber = 1,
        [FromQuery] int PageSize = 20)
        {
            // تحويل DTO إلى Dictionary للفلاتر الموجودة في SQL فقط
            var dict = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(filter.FirstName)) dict["FirstName"] = new[] { filter.FirstName };
            if (!string.IsNullOrWhiteSpace(filter.LastName)) dict["LastName"] = new[] { filter.LastName };
            if (!string.IsNullOrWhiteSpace(filter.Id)) dict["Id"] = new[] { filter.Id };
            if (!string.IsNullOrWhiteSpace(filter.BirthDay)) dict["BirthDay"] = new[] { filter.BirthDay };

            var (totalCount, persons) = await _personRepo.GetAll(dict, sortBy, PageNumber, PageSize, filter.Age);
            var responseData = _mapper.Map<List<ResponsPersonDTO>>(persons);
            return Ok(new
            {
                TotalCount = totalCount,
                Data = responseData
            });


        }
    }
}

