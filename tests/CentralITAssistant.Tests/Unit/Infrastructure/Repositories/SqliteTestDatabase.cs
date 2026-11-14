
using CentralITAssistant.Infrastructure.Persistence.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CentralITAssistant.Tests.Unit.Infrastructure.Repositories;

public sealed class SqliteTestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext Context { get; }

    private SqliteTestDatabase(
        SqliteConnection connection,
        AppDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public static async Task<SqliteTestDatabase> CreateAsync()
    {
        var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);

        await context.Database.EnsureCreatedAsync();

        return new SqliteTestDatabase(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
