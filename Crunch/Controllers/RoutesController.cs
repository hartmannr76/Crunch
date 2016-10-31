using Crunch.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Crunch.Controllers
{
    [Route("api/routes"), UnavailableIfDisconnected]
    public class RoutesController : Controller
    {
        public RoutesController() {
        }
        
        // GET api/values/5
        [HttpGet("{originStation}-{destinationStation}")]
        public ActionResult Get(string originStation, string destinationStation)
        {
                return Ok(new {orig = originStation,
                 destination = destinationStation});
        }
    }
}
