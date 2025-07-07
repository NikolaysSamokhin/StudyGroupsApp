using System.Net.Http.Json;
using StudyGroupsApp.enums;
using StudyGroupsApp.Models;

namespace StudyGroupsApp.Tests.Tests;

[TestFixture]
public class StudyGroupsApiTests
{
    private HttpClient _client;

    [SetUp]
    public void SetUp()
    {
        // Подключаемся к уже запущенному приложению
        _client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000") // Убедись, что порт совпадает
        };
    }

    [Test]
    public async Task GetAllStudyGroups_ReturnsSuccessAndNonEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/study-groups");

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode);

        var groups = await response.Content.ReadFromJsonAsync<List<StudyGroup>>();
        Assert.IsNotNull(groups);
        Assert.IsNotEmpty(groups);
    }

    [Test]
    public async Task CreateStudyGroup_ReturnsCreated()
    {
        // Arrange
        var newGroup = new
        {
            Name = "Physics Group",
            Subject = Subject.Physics,
            CreateDate = DateTime.UtcNow
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/studygroups", newGroup);

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }
}