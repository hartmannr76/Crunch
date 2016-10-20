using NUnit.Framework;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;

namespace Crunch.Integration.Tests.Controllers
{
    [TestFixture]
    public class ExerimentsControllerTests
    {
        private const string ExperimentsRoot = "api/experiments/v1/tests";
        HttpClient _client;

        [SetUp]
        public void SetUp()
        {
            var apiAddress = Environment.GetEnvironmentVariable("DOCKER_TEST_ADDRESS").Trim();
            var cleanedAddress = string.Format("http://{0}:5000", apiAddress);

            _client = new HttpClient {
                BaseAddress = new Uri(cleanedAddress)
            };
        }

        [Test]
        public async Task ConfigureTest_NewTest_HasVersion_1() {
            var r = new Random();
            var name = "foobar-" + r.Next(maxValue: Int32.MaxValue).ToString();

            var test = new TestConfiguration {
                Name = name,
                Variants = new List<Variant>() { new Variant{ Name = "1", Influence = 0.1f } }
            };

            var message = await _client.PostAsJsonAsync(ExperimentsRoot, test);
            Assert.AreEqual(HttpStatusCode.NoContent, message.StatusCode);

            var getRequest = await _client.GetAsync(ExperimentsRoot + "/" + name);
            var parsedResponse = await getRequest.Content.ReadAsAsync<TestConfiguration>();
            Assert.IsNotNull(parsedResponse);
            Assert.AreEqual(1L, parsedResponse.Version);
        }
    }

    public class TestConfiguration {
        public string Name { get; set; }
        public long Version { get; set; }
        public IEnumerable<Variant> Variants { get; set; }
    }   

    public class Variant {
        public string Name { get; set; }
        public float Influence { get; set; }
    }
}