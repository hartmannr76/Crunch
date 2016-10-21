using NUnit.Framework;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Crunch.Integration.Tests.Utils;

namespace Crunch.Integration.Tests.Controllers
{
    [TestFixture]
    internal class ExerimentsControllerTests : ControllerTests
    {
        private const string ExperimentsRoot = "api/experiments/v1/tests";
        HttpClient _client;
        RandomDataGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _client = CreateClient();
            _generator = new RandomDataGenerator();
        }

        [Test]
        public async Task ConfigureTest_NewTest_HasVersion_1() {
            var name = _generator.RandomString();

            var test = new TestConfiguration {
                Name = name,
                Variants = new List<Variant>() { new Variant{ Name = "1", Influence = 0.1f } }
            };

            var message = await _client.PostAsJsonAsync(ExperimentsRoot, test);
            Assert.AreEqual(HttpStatusCode.NoContent, message.StatusCode);

            var getRequest = await _client.GetAsync(ExperimentsRoot + "/" + name);
            var parsedResponse = await getRequest.Content.ReadAsAsync<TestConfiguration>();
            Assert.IsNull(parsedResponse);
            Assert.AreEqual(1L, parsedResponse.Version);
        }

        [Test]
        public async Task ConfigureTest_ExistingTest_IncrementsVersion() {
            var name = _generator.RandomString();

            var test = new TestConfiguration {
                Name = name,
                Variants = new List<Variant>() { new Variant{ Name = "1", Influence = 0.1f } }
            };

            // Creates first version of the test
            await _client.PostAsJsonAsync(ExperimentsRoot, test);
            // Uses the same config to force it to increment
            var message = await _client.PostAsJsonAsync(ExperimentsRoot, test);

            var getRequest = await _client.GetAsync(ExperimentsRoot + "/" + name);
            var parsedResponse = await getRequest.Content.ReadAsAsync<TestConfiguration>();
            Assert.IsNotNull(parsedResponse);
            Assert.AreEqual(2L, parsedResponse.Version);
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
