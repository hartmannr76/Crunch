using Crunch.Contexts;
using Crunch.Repositories;
using Microsoft.AspNetCore.Mvc;
using Crunch.Models.Experiments;
using System.Collections.Generic;
using Crunch.Attributes;

namespace Crunch.Controllers.Experiments
{
    [UnavailableIfDisconnected]
    public class TestsController : Controller {
        private readonly ITestRepository _testRepository;
        private readonly ITestContext _testContext;
        private readonly IParticipantContext _participantContext;

        public TestsController(
            ITestRepository testRepository,
            ITestContext testContext,
            IParticipantContext participantContext) {
            _testRepository = testRepository;
            _testContext = testContext;
            _participantContext = participantContext;
        }

        [HttpPost]
        [Route("api/experiments/v1/tests")]
        public ActionResult ConfigureTest([FromBody] TestConfiguration model)
        {
            _testRepository.ConfigureTest(model.Name, model);

            return NoContent();
        }

        [HttpGet]
        [Route("api/experiments/v1/tests/{test}")]
        public ActionResult GetTest(string test)
        {
            var testConfig = _testContext.GetTest(test);

            return Ok(testConfig);
        }

        [HttpGet]
        [Route("api/experiments/v1/tests/{test}/results")]
        public ActionResult GetTestResults(string test, string[] goal)
        {
            var testConfig = _testContext.GetTest(test);

            var variantData = new List<object>();

            foreach(var variant in testConfig.Variants) {
                var count = _participantContext.GetTotalUserCountForVariant(test, variant.Name, testConfig.Version);

                var conversions = new List<object>();

                foreach(var g in goal) {
                    var goalCount = _testRepository.GetConversionCountForTestAndVariant(test, variant.Name, testConfig.Version, g);
                    conversions.Add(new {goal = g, count = goalCount});
                }

                variantData.Add(new {
                    variant = variant.Name,
                    users = count,
                    conversions = conversions
                });
            }

            return Ok(variantData);
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
