using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using netCoreTest.App;

namespace lirrNetDemo.Controllers
{
    [Route("api/routes")]
    public class RoutesController : Controller
    {
        private readonly ITestClass _testClass;
        public RoutesController(ITestClass testClass) {
            _testClass = testClass;
        }
        
        // GET api/values/5
        [HttpGet("{originStation}-{destinationStation}")]
        public ActionResult Get(string originStation, string destinationStation)
        {
                return Ok(new {orig = _testClass.CallOut(originStation),
                 destination = destinationStation});
        }
    }
}
