using System.Net;
using static AdmissionSystem.Tests.ApiClient;

namespace AdmissionSystem.Tests;

public class ExpiryTests(ShortHoldApiFactory factory) : IClassFixture<ShortHoldApiFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Expired_hold_frees_the_seat_and_cannot_be_confirmed()
    {
        var classId = await client.CreateClassAsync(totalSeats: 1);
        var student = StudentId(1);
        var holdId = await client.ApplySuccessfullyAsync(classId, student);

        Assert.Equal("HELD", (await client.GetHoldAsync(holdId)).Status);
        Assert.Equal(0, (await client.GetClassAsync(classId)).AvailableSeats);

        await Task.Delay(TimeSpan.FromSeconds(3));

        Assert.Equal("EXPIRED", (await client.GetHoldAsync(holdId)).Status);
        Assert.Equal(1, (await client.GetClassAsync(classId)).AvailableSeats);

        var confirm = await client.ConfirmAsync(holdId, student);
        Assert.Equal(HttpStatusCode.Conflict, confirm.StatusCode);

        var anotherStudent = await client.ApplyAsync(classId, StudentId(2));
        Assert.Equal(HttpStatusCode.Created, anotherStudent.StatusCode);
    }
}
