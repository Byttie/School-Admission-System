using Npgsql;

namespace AdmissionSystem.Tests;

public static class TestDatabase
{
    private static string ServerConnectionString =>
        Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
        ?? "Host=localhost;Port=5432;Username=postgres;Password=postgres";

    public static string NewConnectionString()
    {
        var builder = new NpgsqlConnectionStringBuilder(ServerConnectionString)
        {
            Database = $"school_test_{Guid.NewGuid():N}"
        };
        return builder.ConnectionString;
    }

    public static async Task DropAsync(string connectionString)
    {
        var databaseName = new NpgsqlConnectionStringBuilder(connectionString).Database;
        var adminConnectionString = new NpgsqlConnectionStringBuilder(ServerConnectionString)
        {
            Database = "postgres"
        }.ConnectionString;

        NpgsqlConnection.ClearAllPools();

        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            $"""DROP DATABASE IF EXISTS "{databaseName}" WITH (FORCE)""", connection);
        await command.ExecuteNonQueryAsync();
    }
}
