using System.Net;
using System.Net.Http.Json;
using StudyGroupsApp.enums;
using StudyGroupsApp.Models;

namespace StudyGroupsApp.Tests.Client;

/// <summary>
/// Provides methods for interacting with the StudyGroups API endpoints for integration and component testing.
/// </summary>
public class StudyGroupApiClient
{
    /// <summary>
    /// The HTTP client used to send requests to the API.
    /// </summary>
    private readonly HttpClient _client;

    /// <summary>
    /// The base endpoint for study group operations.
    /// </summary>
    private const string BaseEndpoint = "/study-groups";
    /// <summary>
    /// The endpoint template for joining a study group.
    /// </summary>
    private const string JoinGroupEndpoint = "/study-groups/{0}/join?userId={1}";
    /// <summary>
    /// The endpoint template for leaving a study group.
    /// </summary>
    private const string LeaveGroupEndpoint = "/study-groups/{0}/leave?userId={1}";
    /// <summary>
    /// The endpoint template for filtering and sorting study groups.
    /// </summary>
    private const string FilteredSortedGroupsEndpoint = "/study-groups?subject={0}&sort={1}";

    /// <summary>
    /// Initializes a new instance of the <see cref="StudyGroupApiClient"/> class with the specified base URL.
    /// </summary>
    /// <param name="url">The base URL of the StudyGroups API.</param>
    public StudyGroupApiClient(string url)
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(url)
        };
    }

    /// <summary>
    /// Retrieves a list of all study groups, optionally filtered by subject and sorted by creation date.
    /// </summary>
    /// <param name="subject">Optional subject to filter study groups.</param>
    /// <param name="sort">Sort order for creation date ("asc" or "desc").</param>
    /// <returns>A tuple containing the list of study groups and the HTTP status code.</returns>
    public async Task<(List<StudyGroup>? Groups, HttpStatusCode Status)> GetAllStudyGroupsAsync(
        Subject? subject = null, string sort = "asc")
    {
        var url = subject.HasValue
            ? string.Format(FilteredSortedGroupsEndpoint, subject.Value, sort)
            : $"{BaseEndpoint}?sort={sort}";

        var response = await _client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return (null, response.StatusCode);

        var groups = await response.Content.ReadFromJsonAsync<List<StudyGroup>>();

        if (groups is null || groups.Count == 0)
            return (null, response.StatusCode); // Only return status code if no groups

        return (groups, response.StatusCode); 
    }

    /// <summary>
    /// Sends a request to create a new study group.
    /// </summary>
    /// <param name="newGroup">The study group object to create.</param>
    /// <returns>The HTTP response message from the API.</returns>
    public async Task<HttpResponseMessage> CreateStudyGroupAsync(object newGroup)
    {
        return await _client.PostAsJsonAsync(BaseEndpoint, newGroup);
    }

    /// <summary>
    /// Sends a request for a user to join a study group.
    /// </summary>
    /// <param name="groupId">The ID of the study group to join.</param>
    /// <param name="userId">The ID of the user joining the group.</param>
    /// <returns>The HTTP response message from the API.</returns>
    public async Task<HttpResponseMessage> JoinStudyGroupAsync(int groupId, int userId)
    {
        var url = string.Format(JoinGroupEndpoint, groupId, userId);
        return await _client.PostAsync(url, null);
    }

    /// <summary>
    /// Sends a request for a user to leave a study group.
    /// </summary>
    /// <param name="groupId">The ID of the study group to leave.</param>
    /// <param name="userId">The ID of the user leaving the group.</param>
    /// <returns>The HTTP response message from the API.</returns>
    public async Task<HttpResponseMessage> LeaveStudyGroupAsync(int groupId, int userId)
    {
        var url = string.Format(LeaveGroupEndpoint, groupId, userId);
        return await _client.DeleteAsync(url);
    }

    /// <summary>
    /// Deletes all study groups.
    /// </summary>
    /// <returns>The HTTP response message from the API.</returns>
    public async Task<HttpResponseMessage> DeleteAllStudyGroups()
    {
        return await _client.DeleteAsync(BaseEndpoint);
    }
}