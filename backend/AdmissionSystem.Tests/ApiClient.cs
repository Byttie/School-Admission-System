using System.Net.Http.Json;
using AdmissionSystem.Api.Controllers;

namespace AdmissionSystem.Tests;

public static class ApiClient
{
    public static string StudentId(int number) => $"2015{number:D13}";

    public static async Task<Guid> CreateClassAsync(this HttpClient client, int totalSeats)
    {
        var response = await client.PostAsJsonAsync("/classes",
            new { name = $"Test class {Guid.NewGuid():N}", totalSeats });
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<ClassResponse>();
        return created!.Id;
    }

    public static async Task<ClassSeatsResponse> GetClassAsync(this HttpClient client, Guid classId) =>
        (await client.GetFromJsonAsync<ClassSeatsResponse>($"/classes/{classId}"))!;

    public static Task<HttpResponseMessage> ApplyAsync(this HttpClient client, Guid classId, string studentId) =>
        client.SendAsync(WithStudent(HttpMethod.Post, $"/classes/{classId}/holds", studentId));

    public static async Task<Guid> ApplySuccessfullyAsync(this HttpClient client, Guid classId, string studentId)
    {
        var response = await client.ApplyAsync(classId, studentId);
        response.EnsureSuccessStatusCode();

        var hold = await response.Content.ReadFromJsonAsync<HoldCreatedResponse>();
        return hold!.HoldId;
    }

    public static async Task<HoldResponse> GetHoldAsync(this HttpClient client, Guid holdId) =>
        (await client.GetFromJsonAsync<HoldResponse>($"/holds/{holdId}"))!;

    public static Task<HttpResponseMessage> ConfirmAsync(this HttpClient client, Guid holdId, string studentId) =>
        client.SendAsync(WithStudent(HttpMethod.Post, $"/holds/{holdId}/confirm", studentId));

    private static HttpRequestMessage WithStudent(HttpMethod method, string url, string studentId)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add(HoldsController.StudentIdHeader, studentId);
        return request;
    }
}
