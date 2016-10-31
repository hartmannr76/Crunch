using Microsoft.AspNetCore.Mvc;

namespace Crunch.Testing.Controllers
{
    [Route("api/verification")]
    public class TestingController : Controller
    {
        public TestingController() {}
  
        [HttpGet("{originStation}-{destinationStation}")]
        public ActionResult Get(string originStation, string destinationStation)
        {
                return Ok(new {orig = originStation,
                 destination = destinationStation});
        }
    }
}
