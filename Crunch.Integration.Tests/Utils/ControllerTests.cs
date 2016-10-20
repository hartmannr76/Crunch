using System.Net.Http;
using System;

namespace Crunch.Integration.Tests.Utils
{
    internal class ControllerTests
    {
        public HttpClient CreateClient() {
            var apiAddress = Environment.GetEnvironmentVariable("TEST_WEB_ADDRESS").Trim();
            var cleanedAddress = string.Format("http://{0}:5000", apiAddress);

            return new HttpClient {
                BaseAddress = new Uri(cleanedAddress)
            };
        }
    }
}