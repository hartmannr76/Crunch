using System;
using Microsoft.AspNetCore.Mvc;

namespace Crunch.Testing.Controllers
{
    public class ParticipantsController : Controller
    {
        public ParticipantsController() {}
  
        [HttpGet]
        [Route("api/experiments/v1/participants/{clientId}/tests/{testName}")]
        public ActionResult GetParticipationResult(string clientId, string testName, string variant)
        {
            var rand = new Random();

            variant = variant ?? string.Format(
                "{0}-{1}", 
                "foobar", 
                rand.Next(maxValue: Int32.MaxValue).ToString());

            return Ok(new {
                client_id = clientId,
                test = testName, 
                result = variant
            });
        }

        [HttpGet]
        [Route("api/experiments/v1/participants/{clientId}/conversions/{goal}")]
        public ActionResult RecordConversion(string clientId, string goal)
        {
                return NoContent();
        }
    }
}
