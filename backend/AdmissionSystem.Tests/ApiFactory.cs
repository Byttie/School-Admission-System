using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AdmissionSystem.Tests;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string connectionString = TestDatabase.NewConnectionString();

    protected virtual int HoldDurationSeconds => 600;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", connectionString);
        builder.UseSetting("HOLD_DURATION_SECONDS", HoldDurationSeconds.ToString());
    }

    public Task InitializeAsync() => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        await TestDatabase.DropAsync(connectionString);
    }
}

public class ShortHoldApiFactory : ApiFactory
{
    protected override int HoldDurationSeconds => 2;
}
