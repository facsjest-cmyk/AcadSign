using System.Text.Json;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using DownloadUrlResponse = AcadSign.Backend.Web.Endpoints.DownloadUrlResponse;
using DocumentsEndpoint = AcadSign.Backend.Web.Endpoints.Documents;

namespace AcadSign.Backend.Application.UnitTests.API;

[TestFixture]
[Category("P0")]
[Category("Documents")]
public class DocumentDownloadEndpointCompatibilityTests
{
    [Test]
    public async Task GetDownloadUrl_WhenEnvironmentIsDevelopment_ReturnsRawDocumentUrlForDesktopPreview()
    {
        var endpoint = new DocumentsEndpoint();
        var documentId = Guid.NewGuid();

        var dbContext = CreateDbContext();
        var s3Storage = new Mock<IS3StorageService>();
        var auditService = new Mock<IAuditLogService>();
        var logger = new Mock<ILogger<DocumentsEndpoint>>();

        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(x => x.EnvironmentName).Returns(Environments.Development);

        var serviceProvider = new ServiceCollection()
            .AddSingleton(environment.Object)
            .AddLogging()
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost:5000");
        httpContext.Response.Body = new MemoryStream();

        var result = await endpoint.GetDownloadUrl(
            documentId,
            s3Storage.Object,
            dbContext,
            auditService.Object,
            logger.Object,
            httpContext);

        await result.ExecuteAsync(httpContext);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

        var payload = await JsonSerializer.DeserializeAsync<DownloadUrlResponse>(
            httpContext.Response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        payload.Should().NotBeNull();
        payload!.DownloadUrl.Should().Be($"http://localhost:5000/api/v1/documents/{documentId}/raw");
        payload.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddMinutes(59));
        payload.ExpiresAt.Should().BeBefore(DateTime.UtcNow.AddMinutes(61));

        s3Storage.Verify(
            x => x.GeneratePresignedDownloadUrlAsync(It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=dummy;Username=dummy;Password=dummy")
            .Options;

        return new ApplicationDbContext(options);
    }
}
