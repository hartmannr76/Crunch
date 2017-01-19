using Microsoft.AspNetCore.Mvc;

namespace Crunch.Controllers
{
    [Route("healthcheck")]
    public class HealthCheckController : Controller
    {
        public HealthCheckController() {
        }
        
        // GET healthcheck
        [HttpGet]
        public ActionResult HealthCheck()
        {
            // for now, we don't want to check the health since we
            // just want to get this in a running container
            return Ok();
        }
    }
}
