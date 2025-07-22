using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.TestHost;
using prov2.Server.Middlewares;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestProv2
{
    public class TestStartUp
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public TestStartUp()
        {
            // Set up the in-memory test server
            _server = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    // You can configure services like DbContext here for more complex setups
                })
                .Configure(app =>
                {
                    // Add the ExceptionMiddleware to the pipeline
                    app.UseMiddleware<ExceptionMiddleware>();

                    // Add a test endpoint for testing purposes
                    app.Run(async context =>
                    {
                        // Simulate an exception to trigger the middleware
                        throw new System.Exception("Test exception");
                    });
                }));

            _client = _server.CreateClient();
        }

        [Fact]
        public async Task Test_ExceptionMiddleware_Returns_InternalServerError()
        {
            // Act: Send a request to the server (which will throw an exception)
            var response = await _client.GetAsync("/");

            // Assert: Ensure the response status code is 500 (Internal Server Error)
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            // Assert: Ensure the response body contains the expected message
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.Contains("Internal Server Error. Please try again later.", responseBody);
        }
    }
}
