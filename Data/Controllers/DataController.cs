using System.Collections.Generic;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Data.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
     
        public List<Info> Get()
        {
            return new List<Info>() { new Info("Test Data 1"), new Info("Test Data 2"), new Info("Test Data 3"), new Info("Test Data 4"), new Info("Test Data 5") };
        }
    }
}
