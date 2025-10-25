using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserProjectTest.Data;
using UserProjectTest.Data.Model;

namespace TheLastUserProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly UserDbContext userDb;

        public PersonController(UserDbContext userDb)
        {
            this.userDb = userDb;
        }



        [HttpGet]
        public async Task <IActionResult> GetPerson()
        {
            var Person = await userDb.persons.ToListAsync();
            return Ok(Person);
        }

    }
}
