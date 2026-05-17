using System.Net;
using System.Net.Http.Headers;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace tests.Integration.Controllers;

public class GdprTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public GdprTests(CustomWebApplicationFactory factory) =>
        _factory = factory;

    private async Task<(HttpClient client, int userId)> CreateAuthenticatedClientAsync(int userIdSeed)
    {
        int userId;
        string token;

        using (var seedScope = _factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordService = seedScope.ServiceProvider.GetRequiredService<PasswordService>();
            var jwtService = seedScope.ServiceProvider.GetRequiredService<JwtService>();

            var user = new User
            {
                Id = userIdSeed,
                Username = $"gdpr_user_{userIdSeed}",
                Email = $"gdpr_{userIdSeed}@test.com",
                Password = passwordService.HashPassword("Password123!")
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            userId = user.Id;
            token = jwtService.GenerateToken(user);
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, userId);
    }

    [Fact]
    public async Task ExportMyData_WithAuth_ReturnsJsonFileWithUserData()
    {
        // Arrange
        var (client, userId) = await CreateAuthenticatedClientAsync(5001);

        // Act
        var response = await client.GetAsync("/api/users/export");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var disposition = response.Content.Headers.ContentDisposition;
        Assert.NotNull(disposition);
        Assert.StartsWith("trivia-data-export-", disposition!.FileName);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains($"\"UserId\": {userId}", json);
        Assert.Contains($"gdpr_{userId}@test.com", json);
    }

    [Fact]
    public async Task ExportMyData_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/users/export");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMyAccount_WithAuth_DeletesUserAndReturnsOk()
    {
        // Arrange
        var (client, userId) = await CreateAuthenticatedClientAsync(5101);

        // Act
        var response = await client.DeleteAsync("/api/users/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var verifyScope = _factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FindAsync(userId);
        Assert.Null(user);
    }

    [Fact]
    public async Task DeleteMyAccount_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/users/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}