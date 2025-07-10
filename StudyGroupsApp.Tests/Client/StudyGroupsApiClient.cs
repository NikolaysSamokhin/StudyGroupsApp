using System.Net.Http.Json;
using StudyGroupsApp.enums;
using StudyGroupsApp.Models;

namespace StudyGroupsApp.Tests.Client;

/// <summary>
/// Client for interacting with the StudyGroups API endpoints.
/// </summary>
public class StudyGroupApiClient
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="StudyGroupApiClient"/> class.
    /// </summary>
    /// <param name="client">The HTTP client to use for requests.</param>
    public StudyGroupApiClient(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Retrieves a list of all study groups, optionally filtered by subject and sorted by creation date.
    /// </summary>
    /// <param name="subject">Optional subject filter (Math, Chemistry, or Physics).</param>
    /// <param name="sort">Sort order: "asc" (oldest first) or "desc" (newest first). Default is "asc".</param>
    /// <returns>A list of study groups matching the filter criteria.</returns>
    public async Task<List<StudyGroup>?> GetAllStudyGroupsAsync(Subject? subject = null, string sort = "asc")
    {
        var url = "/study-groups";

        if (subject.HasValue)
        {
            url += $"?subject={subject.Value}&sort={sort}";
        }
        else if (!string.IsNullOrWhiteSpace(sort))
        {
            url += $"?sort={sort}";
        }

        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<StudyGroup>>();
    }

    /// <summary>
    /// Sends a request to create a new study group.
    /// </summary>
    /// <param name="newGroup">The study group data object to create (e.g., name, subject, createDate).</param>
    /// <returns>The HTTP response from the API.</returns>
    public async Task<HttpResponseMessage> CreateStudyGroupAsync(object newGroup)
    {
        return await _client.PostAsJsonAsync("/study-groups", newGroup);
    }

    /// <summary>
    /// Sends a request for a user to join a study group.
    /// </summary>
    /// <param name="groupId">The ID of the study group.</param>
    /// <param name="userId">The ID of the user to join the group.</param>
    /// <returns>The HTTP response from the API.</returns>
    public async Task<HttpResponseMessage> JoinStudyGroupAsync(int groupId, int userId)
    {
        var url = $"/study-groups/{groupId}/join?userId={userId}";
        return await _client.PostAsync(url, null);
    }

    /// <summary>
    /// Sends a request for a user to leave a study group.
    /// </summary>
    /// <param name="groupId">The ID of the study group.</param>
    /// <param name="userId">The ID of the user leaving the group.</param>
    /// <returns>The HTTP response from the API.</returns>
    public async Task<HttpResponseMessage> LeaveStudyGroupAsync(int groupId, int userId)
    {
        var url = $"/study-groups/{groupId}/leave?userId={userId}";
        return await _client.DeleteAsync(url);
    }
}