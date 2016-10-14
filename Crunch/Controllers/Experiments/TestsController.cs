using Crunch.Services;
using Microsoft.AspNetCore.Mvc;
using Crunch.Models.Experiments;

namespace Crunch.Controllers.Experiments
{
    public class TestsController : Controller {
        private readonly IDBConnector _db;

        public TestsController(IDBConnector db) {
            _db = db;
        }

        [HttpPost]
        [Route("api/experiments/v1/tests")]
        public ActionResult ConfigureTest([FromBody] TestConfiguration model)
        {
            _db.ConfigureTest(model.Name, model);

            return NoContent();
        }

        [HttpGet]
        [Route("api/experiments/v1/tests/{test}")]
        public ActionResult GetTest(string test)
        {
            var testConfig = _db.GetTest(test);
            _db.SetConnection(false);

            return Ok(testConfig);
        }

        [HttpGet]
        [Route("api/experiments/v1/tests/{test}/results")]
        public ActionResult GetTestResults(string test, string version)
        {
            var testConfig = _db.GetTest(test);

            return Ok(testConfig);
        }
    }
}


// Active experiments should be able to be fetched from client application to use cache
// Get active AB's for user             /experiments/v1/participants/{:id}/active GET
// Create AB configuration for user     /experiments/v1/participants/{:id} POST
// Trigger goal conversion for user     /experiments/v1/participants/{:id}/
// Get experiment for user              /experiments/v1/participants/{:id}/goals/{:goal_id} GET
// Get all active experiments           /experiments/v1/?active
// Get single active experiment         /experiments/v1/{:experiment_id}

/*
    experiment: {
        experiment_id: long,
        versions: [
            version {
                variants: [ variant {
                    key: string,
                    value: int % of experiment,
                    amount_of_uses: long,
                    goals: [
                        goal: {
                            key: string,
                            value: long
                        }
                    ]
                }],
                start_time: string,
                end_time: string,
                version_number: long
            }
        ]
    }

    experiment_configuration: {
        experiment_id: long,
            variants: [ variant {
                key: string,
                value: int % of experiment,
                amount_of_uses: long,
                goals: [
                    goal: {
                        key: string,
                        value: long
                    }
                ]
            }],
            start_time: string,
            end_time: string,
            version_number: long
        }
    }
*/
