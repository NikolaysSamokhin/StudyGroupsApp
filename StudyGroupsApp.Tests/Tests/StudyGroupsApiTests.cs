using StudyGroupsApp.enums;
using StudyGroupsApp.Tests.Client;

namespace StudyGroupsApp.Tests.Tests;

[TestFixture]
public class StudyGroupsApiTests
{
    private HttpClient _client;
    private StudyGroupApiClient _apiClient;

    [SetUp]
    public void SetUp()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
        _apiClient = new StudyGroupApiClient(_client);
    }

    [Test]
    public async Task GetAllStudyGroupsReturnsSuccessAndNonEmptyListAsyncTest()
    {
        var groups = await _apiClient.GetAllStudyGroupsAsync();
        Assert.IsNotNull(groups);
        Assert.IsNotEmpty(groups);
    }

    [Test]
    public async Task CreateStudyGroupReturnsCreatedAsyncTest()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"); // e.g. 20250710145530
        var name = $"Group_{timestamp}";
        
        var newGroup = new
        {
            Name = name,
            Subject = Subject.Physics,
            CreateDate = DateTime.UtcNow
        };

        var response = await _apiClient.CreateStudyGroupAsync(newGroup);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }
}