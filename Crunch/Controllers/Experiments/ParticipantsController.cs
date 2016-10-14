using System;
using Crunch.Services;
using Microsoft.AspNetCore.Mvc;

namespace Crunch.Controllers.Experiments
{
    public class ParticipantsController : Controller
    {
        private readonly IDBConnector _db;
        public ParticipantsController(IDBConnector db) {
            _db = db;
        }
        
        // GET api/values/5
        [HttpGet]
        [Route("api/experiments/v1/participants/{clientId}/tests/{testName}")]
        public ActionResult GetParticipationResult(string clientId, string testName)
        {
                if(string.IsNullOrEmpty(clientId)) {
                    clientId = Guid.NewGuid().ToString();
                }

                var variant = _db.EnrollParticipantInTest(clientId, testName);

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
                if(string.IsNullOrEmpty(clientId)) {
                    clientId = Guid.NewGuid().ToString();
                }

                _db.RecordGoal(clientId, goal);

                return NoContent();
        }
    }
}
