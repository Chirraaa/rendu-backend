using KanbanApp.API.Data;
using KanbanApp.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KanbanApp.IntegrationTests;

public class KanbanWebAppFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _connection;

    public KanbanWebAppFactory()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", "test-secret-key-that-is-long-enough-32chars");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");
        Environment.SetEnvironmentVariable("ACCESS_TOKEN_EXPIRY_MINUTES", "15");
        Environment.SetEnvironmentVariable("REFRESH_TOKEN_EXPIRY_DAYS", "7");
        Environment.SetEnvironmentVariable("REMEMBER_ME_EXPIRY_DAYS", "30");

        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        if (!db.Users.Any(u => u.Email == "admin@kanban.com"))
        {
            db.Users.Add(new User
            {
                Email = "admin@kanban.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("passwordAdmin"),
                FirstName = "Adam",
                LastName = "Admin",
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}
