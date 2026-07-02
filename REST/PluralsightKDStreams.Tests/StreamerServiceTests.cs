using NUnit.Framework;
using Moq;
using PluralsightKDStreams.Services;
using PluralsightKDStreams.Dtos;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PluralsightKDStreams.Tests
{
    [TestFixture]
    public class StreamerServiceTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory = null!;
        private Mock<ILogger<StreamerService>> _mockLogger = null!;
        private StreamerService _service = null!;

        [SetUp]
        public void Setup()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<StreamerService>>();
            _service = new StreamerService(_mockHttpClientFactory.Object, _mockLogger.Object);
        }

        #region GetPosterServerAsync Tests

        [Test]
        public async Task GetPosterServerAsync_WithValidPosterId_ReturnsPosterDto()
        {
            // Arrange
            string posterId = "poster-123";

            // Act
            PosterDto result = await _service.GetPosterServerAsync(posterId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(posterId));
            Assert.That(result.Description, Is.EqualTo("Generated poster"));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data.Length, Is.EqualTo(1024 * 1024 * 5));
        }

        [Test]
        public async Task GetPosterServerAsync_GeneratedDataNotEmpty()
        {
            // Arrange
            string posterId = "test-poster";

            // Act
            PosterDto result = await _service.GetPosterServerAsync(posterId);

            // Assert
            Assert.That(result.Data, Is.Not.Empty);
        }

        #endregion

        #region GetPosterClientAsync Tests

        [Test]
        public async Task GetPosterClientAsync_WithSuccessfulResponse_ReturnsPosterDto()
        {
            // Arrange
            string posterId = "poster-456";
            var posterDto = new PosterDto(posterId, "Test poster", new byte[] { 1, 2, 3 });
            var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            var json = JsonSerializer.Serialize(posterDto, jsonOptions);

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Local"))
                .Returns(() =>
                {
                    var client = new HttpClient(new MockHttpMessageHandler(mockResponse));
                    client.BaseAddress = new Uri("http://localhost/");
                    return client;
                });

            // Act
            PosterDto? result = await _service.GetPosterClientAsync(posterId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Id, Is.EqualTo(posterId));
            _mockHttpClientFactory.Verify(x => x.CreateClient("Local"), Times.Once);
        }

        [Test]
        public async Task GetPosterClientAsync_WithFailedResponse_ReturnsNull()
        {
            // Arrange
            string posterId = "poster-789";
            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Local"))
                .Returns(() =>
                {
                    var client = new HttpClient(new MockHttpMessageHandler(mockResponse));
                    client.BaseAddress = new Uri("http://localhost/");
                    return client;
                });

            // Act
            PosterDto? result = await _service.GetPosterClientAsync(posterId);

            // Assert
            Assert.That(result, Is.Null);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to get poster")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task GetPosterClientAsync_WithHttpRequestException_ReturnsNull()
        {
            // Arrange
            string posterId = "poster-error";

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Local"))
                .Returns(() => new HttpClient(new ThrowingHttpMessageHandler(new HttpRequestException("Connection failed"))));

            // Act
            PosterDto? result = await _service.GetPosterClientAsync(posterId);

            // Assert
            Assert.That(result, Is.Null);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region CreatePosterServerAsync Tests

        [Test]
        public async Task CreatePosterServerAsync_WithValidPoster_ReturnsTrue()
        {
            // Arrange
            var poster = new PosterDto("poster-create", "New poster", new byte[] { 1, 2, 3 });

            // Act
            bool result = await _service.CreatePosterServerAsync(poster);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(poster.Id, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task CreatePosterServerAsync_WithNullData_ReturnsFalse()
        {
            // Arrange
            var poster = new PosterDto("poster-null", "Invalid poster", null!);

            // Act
            bool result = await _service.CreatePosterServerAsync(poster);

            // Assert
            Assert.That(result, Is.False);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid poster data")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task CreatePosterServerAsync_GeneratesUniqueId()
        {
            // Arrange
            var poster1 = new PosterDto("poster1", "Poster 1", new byte[] { 1 });
            var poster2 = new PosterDto("poster2", "Poster 2", new byte[] { 2 });

            // Act
            await _service.CreatePosterServerAsync(poster1);
            await _service.CreatePosterServerAsync(poster2);

            // Assert
            Assert.That(poster1.Id, Is.Not.EqualTo(poster2.Id));
            Assert.That(Guid.TryParse(poster1.Id, out _), Is.True);
            Assert.That(Guid.TryParse(poster2.Id, out _), Is.True);
        }

        #endregion

        #region CreatePosterClientAsync Tests

        [Test]
        public async Task CreatePosterClientAsync_WithSuccessfulResponse_ReturnsLocationHeader()
        {
            // Arrange
            var poster = new PosterDto("poster-id", "Test poster", new byte[] { 1, 2, 3 });
            var expectedLocation = "http://example.com/posters/new-id";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.Created);
            mockResponse.Headers.Location = new Uri(expectedLocation);

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Local"))
                .Returns(() =>
                {
                    var client = new HttpClient(new MockHttpMessageHandler(mockResponse));
                    client.BaseAddress = new Uri("http://localhost/");
                    return client;
                });

            // Act
            string result = await _service.CreatePosterClientAsync(poster);

            // Assert
            Assert.That(result, Is.EqualTo(expectedLocation));
        }

        [Test]
        public async Task CreatePosterClientAsync_WithFailedResponse_ReturnsEmptyString()
        {
            // Arrange
            var poster = new PosterDto("poster-id", "Test poster", new byte[] { 1, 2, 3 });
            var mockResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Local"))
                .Returns(() =>
                {
                    var client = new HttpClient(new MockHttpMessageHandler(mockResponse));
                    client.BaseAddress = new Uri("http://localhost/");
                    return client;
                });

            // Act
            string result = await _service.CreatePosterClientAsync(poster);

            // Assert
            Assert.That(result, Is.Empty);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create poster")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task CreatePosterClientAsync_WithHttpRequestException_ReturnsEmptyString()
        {
            // Arrange
            var poster = new PosterDto("poster-id", "Test poster", new byte[] { 1, 2, 3 });

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Local"))
                .Returns(() => new HttpClient(new ThrowingHttpMessageHandler(new HttpRequestException("Network error"))));

            // Act
            string result = await _service.CreatePosterClientAsync(poster);

            // Assert
            Assert.That(result, Is.Empty);
        }

        #endregion

        #region GetCompressedPosterServerAsync Tests

        [Test]
        public async Task GetCompressedPosterServerAsync_WithValidPosterId_ReturnsCompressedMemoryStream()
        {
            // Arrange
            string posterId = "poster-compressed";

            // Act
            MemoryStream? result = await _service.GetCompressedPosterServerAsync(posterId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
            Assert.That(result.Position, Is.EqualTo(0));
        }

        [Test]
        public async Task GetCompressedPosterServerAsync_ReturnedStreamIsReadable()
        {
            // Arrange
            string posterId = "poster-readable";

            // Act
            MemoryStream? result = await _service.GetCompressedPosterServerAsync(posterId);

            // Assert
            Assert.That(result?.CanRead, Is.True);
        }

        #endregion

        #region GetCompressedPosterClientAsync Tests

        [Test]
        public async Task GetCompressedPosterClientAsync_WithSuccessfulResponse_ReturnsPosterDto()
        {
            // Arrange
            string posterId = "poster-compressed-client";
            var posterDto = new PosterDto(posterId, "Compressed poster", new byte[] { 1, 2, 3 });
            var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            var json = JsonSerializer.Serialize(posterDto, jsonOptions);

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Local"))
                .Returns(() =>
                {
                    var client = new HttpClient(new MockHttpMessageHandler(mockResponse));
                    client.BaseAddress = new Uri("http://localhost/");
                    return client;
                });

            // Act
            PosterDto? result = await _service.GetCompressedPosterClientAsync(posterId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Id, Is.EqualTo(posterId));
        }

        [Test]
        public async Task GetCompressedPosterClientAsync_WithFailedResponse_ReturnsNull()
        {
            // Arrange
            string posterId = "poster-not-found";
            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Local"))
                .Returns(() =>
                {
                    var client = new HttpClient(new MockHttpMessageHandler(mockResponse));
                    client.BaseAddress = new Uri("http://localhost/");
                    return client;
                });

            // Act
            PosterDto? result = await _service.GetCompressedPosterClientAsync(posterId);

            // Assert
            Assert.That(result, Is.Null);
        }

        #endregion

        #region CreateCompressedPosterServerAsync Tests

        [Test]
        public async Task CreateCompressedPosterServerAsync_WithValidPoster_ReturnsTrue()
        {
            // Arrange
            var poster = new PosterDto("poster-comp", "Compressed poster", new byte[] { 1, 2, 3 });

            // Act
            bool result = await _service.CreateCompressedPosterServerAsync(poster);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(poster.Id, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task CreateCompressedPosterServerAsync_WithNullData_ReturnsFalse()
        {
            // Arrange
            var poster = new PosterDto("poster-null-comp", "Invalid", null!);

            // Act
            bool result = await _service.CreateCompressedPosterServerAsync(poster);

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region CreateCompressedPosterClientAsync Tests

        [Test]
        public async Task CreateCompressedPosterClientAsync_WithSuccessfulResponse_ReturnsLocationHeader()
        {
            // Arrange
            var poster = new PosterDto("poster-id", "Test poster", new byte[] { 1, 2, 3 });
            var expectedLocation = "http://example.com/posters/compressed-id";

            var mockResponse = new HttpResponseMessage(HttpStatusCode.Created);
            mockResponse.Headers.Location = new Uri(expectedLocation);

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Local"))
                .Returns(() =>
                {
                    var client = new HttpClient(new MockHttpMessageHandler(mockResponse));
                    client.BaseAddress = new Uri("http://localhost/");
                    return client;
                });

            // Act
            string result = await _service.CreateCompressedPosterClientAsync(poster);

            // Assert
            Assert.That(result, Is.EqualTo(expectedLocation));
        }

        [Test]
        public async Task CreateCompressedPosterClientAsync_WithFailedResponse_ReturnsEmptyString()
        {
            // Arrange
            var poster = new PosterDto("poster-id", "Test poster", new byte[] { 1, 2, 3 });
            var mockResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Local"))
                .Returns(() =>
                {
                    var client = new HttpClient(new MockHttpMessageHandler(mockResponse));
                    client.BaseAddress = new Uri("http://localhost/");
                    return client;
                });

            // Act
            string result = await _service.CreateCompressedPosterClientAsync(poster);

            // Assert
            Assert.That(result, Is.Empty);
        }

        #endregion

        #region GetServerTrailerAsync Tests

        [Test]
        public async Task GetServerTrailerAsync_WithValidTrailerId_ReturnsTrue()
        {
            // Arrange
            string trailerId = "trailer-123";
            using var cts = new CancellationTokenSource();

            // Act
            bool result = await _service.GetServerTrailerAsync(trailerId, cts.Token);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetServerTrailerAsync_WithCancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            string trailerId = "trailer-cancelled";
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            Assert.ThrowsAsync<OperationCanceledException>(
                async () => await _service.GetServerTrailerAsync(trailerId, cts.Token)
            );
        }

        #endregion

        #region GetClientTrailerAsync Tests

        [Test]
        public async Task GetClientTrailerAsync_WithSuccessfulResponse_ReturnsTrue()
        {
            // Arrange
            string trailerId = "trailer-456";
            int timeout = 30;
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK);

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Local"))
                .Returns(() =>
                {
                    var client = new HttpClient(new MockHttpMessageHandler(mockResponse));
                    client.BaseAddress = new Uri("http://localhost/");
                    return client;
                });

            using var cts = new CancellationTokenSource();

            // Act
            bool result = await _service.GetClientTrailerAsync(trailerId, timeout, cts.Token);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task GetClientTrailerAsync_WithFailedResponse_ReturnsFalse()
        {
            // Arrange
            string trailerId = "trailer-failed";
            int timeout = 30;
            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Local"))
                .Returns(() =>
                {
                    var client = new HttpClient(new MockHttpMessageHandler(mockResponse));
                    client.BaseAddress = new Uri("http://localhost/");
                    return client;
                });

            using var cts = new CancellationTokenSource();

            // Act
            bool result = await _service.GetClientTrailerAsync(trailerId, timeout, cts.Token);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task GetClientTrailerAsync_WithTimeoutSet_AppliesTimeoutToClient()
        {
            // Arrange
            string trailerId = "trailer-timeout";
            int timeout = 15;
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK);
            HttpClient? capturedClient = null;

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Local"))
                .Returns(() =>
                {
                    capturedClient = new HttpClient(new MockHttpMessageHandler(mockResponse));
                    capturedClient.BaseAddress = new Uri("http://localhost/");
                    return capturedClient;
                });

            using var cts = new CancellationTokenSource();

            // Act
            await _service.GetClientTrailerAsync(trailerId, timeout, cts.Token);

            // Assert
            Assert.That(capturedClient?.Timeout, Is.EqualTo(TimeSpan.FromSeconds(timeout)));
        }

        [Test]
        public void GetClientTrailerAsync_WithCancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            string trailerId = "trailer-cancelled";
            int timeout = 30;
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK);

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Local"))
                .Returns(() =>
                {
                    var client = new HttpClient(new ThrowingHttpMessageHandler(new OperationCanceledException()));
                    client.BaseAddress = new Uri("http://localhost/");
                    return client;
                });

            using var cts = new CancellationTokenSource();

            // Act & Assert
            Assert.ThrowsAsync<OperationCanceledException>(
                async () => await _service.GetClientTrailerAsync(trailerId, timeout, cts.Token)
            );
        }

        #endregion
    }

    // Helper classes for mocking HttpClient
    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public MockHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }

    internal class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public ThrowingHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw _exception;
        }
    }
}
