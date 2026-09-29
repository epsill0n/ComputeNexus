using ComputeNexus.Domain.Entities;
using ComputeNexus.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ComputeNexus.Tests.IntegrationTests.Fixtures;

public class WebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private IServiceScope _scope = null!;
    public ApplicationDbContext DbContext { get; private set; } = null!;
    public UserManager<User> UserManager { get; private set; } = null!;
    public string TestUserId { get; private set; } = "test-user-id-123";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // remove existing dbcontext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // add in-memory SQLite
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));

            // override background services
            services.RemoveAll(typeof(Microsoft.Extensions.Hosting.IHostedService));

            // override authentication to use test handler

            var authDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IAuthenticationSchemeProvider));
            if (authDescriptor != null)
            {
                services.Remove(authDescriptor);
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }

    public async Task InitializeAsync()
    {
        _scope = Services.CreateScope();
        var sp = _scope.ServiceProvider;
        
        DbContext = sp.GetRequiredService<ApplicationDbContext>();
        UserManager = sp.GetRequiredService<UserManager<User>>();

        await DbContext.Database.EnsureCreatedAsync();

        // create a test user that matches the TestAuthHandler claims
        var user = new User
        {
            Id = TestUserId,
            UserName = "testuser",
            Email = "test@example.com",
            EmailConfirmed = true
        };

        var result = await UserManager.CreateAsync(user, "Password123!");
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create test user: {string.Join(", ", result.Errors)}");
        }
    }

    public new async Task DisposeAsync()
    {
        _scope?.Dispose();
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}