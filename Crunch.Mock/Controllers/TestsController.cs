using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Crunch.Shared.Models.Experiments;

namespace Crunch.Testing.Controllers
{
    public class TestsController : Controller
    {
        private TestConfiguration mockTest = new TestConfiguration {
            Name = "MockTest",
            Version = 1,
            Variants = new List<Variant>() {
                { new Variant { Name = "Variant-A", Influence = 0.5f } },
                { new Variant { Name = "Variant-B", Influence = 0.5f }}
            }
        };

        private Dictionary<string, int> mockGoals = new Dictionary<string, int>() {
            { Key = "Goal-A", Value = 10 },
            { Key = "Goal-B", Value = 20 }
        };

        private Dictionary<string, int> mockVariantCountLookup = new Dictionary<string, int>() {
            { Key = "Variant-A", Value = 10 },
            { Key = "Variant-B", Value = 20 }
        };

        public TestsController() {}
  
        [HttpPost]
        [Route("api/experiments/v1/tests")]
        public ActionResult ConfigureTest([FromBody] TestConfiguration model)
        {
            return NoContent();
        }

        [HttpGet]
        [Route("api/experiments/v1/tests/{test}")]
        public ActionResult GetTest(string test)
        {
            if (test == "mock-test") {
                return Ok(mockTest);
            }

            return Ok();
        }

        [HttpGet]
        [Route("api/experiments/v1/tests/{test}/results")]
        public ActionResult GetTestResults(string test, string[] goal)
        {
            var variantData = new List<object>();

            if(test == "mock-test") {
                foreach(var variant in mockTest.Variants) {
                    var conversions = new List<object>();

                    foreach(var g in goal) {
                        if(mockGoals.ContainsKey(g)) {
                            conversions.Add(new {goal = g, count = mockGoals[g]});
                        }
                    }

                    variantData.Add(new {
                        variant = variant.Name,
                        users = mockVariantCountLookup[variant.Name],
                        conversions = conversions
                    });
                }
            }

            return Ok(variantData);
        }
    }
}
