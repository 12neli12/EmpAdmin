using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using prov2.Server.Middlewares;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Text.Json;

namespace prov2.Server.Tests.Middleware
{
    public class ExceptionMiddlewareTests
    {
        // Test: Ensure next middleware is called when no exception occurs
        [Fact]
        public async Task InvokeAsync_Should_CallNextMiddleware_When_NoExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            var nextMiddlewareMock = new Mock<RequestDelegate>();
            var httpContext = new DefaultHttpContext();

            var middleware = new ExceptionMiddleware(nextMiddlewareMock.Object, loggerMock.Object);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert: Ensure the next middleware is invoked once
            nextMiddlewareMock.Verify(x => x.Invoke(httpContext), Times.Once);
        }

        // Test: Ensure exception is logged and response is 500 Internal Server Error
        [Fact]
        public async Task InvokeAsync_Should_LogError_And_ReturnInternalServerError_When_ExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            var nextMiddlewareMock = new Mock<RequestDelegate>();
            nextMiddlewareMock
                .Setup(x => x.Invoke(It.IsAny<HttpContext>()))
                .ThrowsAsync(new Exception("Test exception"));

            var httpContext = new DefaultHttpContext();
            var responseStream = new MemoryStream();
            httpContext.Response.Body = responseStream;

            var middleware = new ExceptionMiddleware(nextMiddlewareMock.Object, loggerMock.Object);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert: Ensure the exception was logged
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Something went wrong: Test exception"), Times.Once);

            // Assert: Check the response body contains the error message
            responseStream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(responseStream, Encoding.UTF8))
            {
                var responseBody = await reader.ReadToEndAsync();
                Assert.Contains("Internal Server Error. Please try again later.", responseBody);
            }

            // Assert: Verify the response status code is 500 (InternalServerError)
            Assert.Equal((int)HttpStatusCode.InternalServerError, httpContext.Response.StatusCode);
        }

        // Test: Ensure response is returned in JSON format with correct fields
        [Fact]
        public async Task InvokeAsync_Should_ReturnJsonErrorResponse_When_ExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            var nextMiddlewareMock = new Mock<RequestDelegate>();
            nextMiddlewareMock
                .Setup(x => x.Invoke(It.IsAny<HttpContext>()))
                .ThrowsAsync(new Exception("Test exception"));

            var httpContext = new DefaultHttpContext();
            var responseStream = new MemoryStream();
            httpContext.Response.Body = responseStream;

            var middleware = new ExceptionMiddleware(nextMiddlewareMock.Object, loggerMock.Object);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Log the response body for debugging
            responseStream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(responseStream, Encoding.UTF8))
            {
                var responseBody = await reader.ReadToEndAsync();
                Console.WriteLine($"Response Body: {responseBody}");

                // Parse the response as JSON
                var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);

                // Check status code and message in the response
                Assert.Equal(500, jsonResponse["StatusCode"]);
                Assert.Equal("Internal Server Error. Please try again later.", jsonResponse["Message"]);
            }

            // Assert: Verify the response status code is 500 (InternalServerError)
            Assert.Equal((int)HttpStatusCode.InternalServerError, httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Should_HandleEmptyExceptionMessage_Gracefully()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            var nextMiddlewareMock = new Mock<RequestDelegate>();
            nextMiddlewareMock
                .Setup(x => x.Invoke(It.IsAny<HttpContext>()))
                .ThrowsAsync(new Exception(string.Empty));  // Simulate an exception with an empty message

            var httpContext = new DefaultHttpContext();
            var responseStream = new MemoryStream();
            httpContext.Response.Body = responseStream;

            var middleware = new ExceptionMiddleware(nextMiddlewareMock.Object, loggerMock.Object);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert: Ensure the exception was logged with an empty message
            loggerMock.Verify(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Something went wrong:")),
                It.IsAny<object[]>()), Times.Once);

            // Assert: Check the response body contains the error message
            responseStream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(responseStream, Encoding.UTF8))
            {
                var responseBody = await reader.ReadToEndAsync();
                Assert.Contains("Internal Server Error. Please try again later.", responseBody);
            }

            // Assert: Verify the response status code is 500 (InternalServerError)
            Assert.Equal((int)HttpStatusCode.InternalServerError, httpContext.Response.StatusCode);
        }

    }
}
