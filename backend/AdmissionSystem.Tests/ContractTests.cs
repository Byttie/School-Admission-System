using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static AdmissionSystem.Tests.ApiClient;

namespace AdmissionSystem.Tests;

public class ContractTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Create_class_returns_201_with_contract_fields()
    {
        var response = await client.PostAsJsonAsync("/classes",
            new { name = "Class 6 - Morning Shift", totalSeats = 120 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("id", out _));
        Assert.Equal("Class 6 - Morning Shift", json.GetProperty("name").GetString());
        Assert.Equal(120, json.GetProperty("totalSeats").GetInt32());
    }

    [Fact]
    public async Task Create_class_rejects_invalid_input()
    {
        var blankName = await client.PostAsJsonAsync("/classes", new { name = "  ", totalSeats = 10 });
        var zeroSeats = await client.PostAsJsonAsync("/classes", new { name = "Class 7", totalSeats = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, blankName.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, zeroSeats.StatusCode);
    }

    [Fact]
    public async Task Apply_requires_a_valid_student_id()
    {
        var classId = await client.CreateClassAsync(totalSeats: 5);

        var missing = await client.PostAsync($"/classes/{classId}/holds", null);
        var tooShort = await client.ApplyAsync(classId, "12345");

        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, tooShort.StatusCode);
    }

    [Fact]
    public async Task Only_the_owner_can_confirm_a_hold()
    {
        var classId = await client.CreateClassAsync(totalSeats: 5);
        var owner = StudentId(1);
        var holdId = await client.ApplySuccessfullyAsync(classId, owner);

        var byOther = await client.ConfirmAsync(holdId, StudentId(2));
        var byOwner = await client.ConfirmAsync(holdId, owner);

        Assert.Equal(HttpStatusCode.Forbidden, byOther.StatusCode);
        Assert.Equal(HttpStatusCode.OK, byOwner.StatusCode);
        Assert.Equal("CONFIRMED", (await client.GetHoldAsync(holdId)).Status);
    }
}
