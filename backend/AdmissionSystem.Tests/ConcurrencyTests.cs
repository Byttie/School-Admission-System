using System.Net;
using static AdmissionSystem.Tests.ApiClient;

namespace AdmissionSystem.Tests;

public class ConcurrencyTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Many_students_applying_at_once_never_exceed_the_seat_limit()
    {
        var classId = await client.CreateClassAsync(totalSeats: 5);

        var responses = await Task.WhenAll(
            Enumerable.Range(1, 50).Select(i => client.ApplyAsync(classId, StudentId(i))));

        Assert.Equal(5, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(45, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));

        var seats = await client.GetClassAsync(classId);
        Assert.Equal(5, seats.HeldSeats);
        Assert.Equal(0, seats.AvailableSeats);
    }

    [Fact]
    public async Task Same_student_applying_at_once_gets_only_one_hold()
    {
        var classId = await client.CreateClassAsync(totalSeats: 10);
        var student = StudentId(1);

        var responses = await Task.WhenAll(
            Enumerable.Range(1, 10).Select(_ => client.ApplyAsync(classId, student)));

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(9, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));

        var seats = await client.GetClassAsync(classId);
        Assert.Equal(1, seats.HeldSeats);
    }

    [Fact]
    public async Task Confirming_many_times_at_once_counts_the_seat_once()
    {
        var classId = await client.CreateClassAsync(totalSeats: 3);
        var student = StudentId(1);
        var holdId = await client.ApplySuccessfullyAsync(classId, student);

        var responses = await Task.WhenAll(
            Enumerable.Range(1, 10).Select(_ => client.ConfirmAsync(holdId, student)));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));

        var seats = await client.GetClassAsync(classId);
        Assert.Equal(1, seats.ConfirmedSeats);
        Assert.Equal(0, seats.HeldSeats);
        Assert.Equal(2, seats.AvailableSeats);
    }
}
