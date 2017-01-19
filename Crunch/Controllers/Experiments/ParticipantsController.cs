using System;
using Crunch.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Crunch.Controllers.Experiments
{
    public class ParticipantsController : Controller
    {
        private readonly IParticipantRepository _participantRepository;
        public ParticipantsController(IParticipantRepository participantRepository) {
            _participantRepository = participantRepository;
        }
        
        // GET api/values/5
        [HttpGet]
        [Route("api/experiments/v1/participants/{clientId}/tests/{testName}")]
        public ActionResult GetParticipationResult(string clientId, string testName)
        {
                if(string.IsNullOrEmpty(clientId)) {
                    clientId = Guid.NewGuid().ToString();
                }

                var variant = _participantRepository.EnrollParticipantInTest(clientId, testName);

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

                _participantRepository.RecordGoal(clientId, goal);

                return NoContent();
        }
    }
}
