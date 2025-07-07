using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace StudyGroupsApp.Tests;

public class StudyGroupApiTests
{
    private HttpClient? _client;


    [SetUp]
    public void SetUp()
    {
        _client = new HttpClient { BaseAddress = new Uri("https://localhost:5000") };
    }
    
    [Test]
    public async Task CreateStudyGroupValidInputReturnsOk()
    {
        var group = new
        {
            Name = "Physics Buddies",
            Subject = "Physics",
            CreateDate = DateTime.UtcNow
        };

        var content = new StringContent(JsonConvert.SerializeObject(group), Encoding.UTF8, "application/json");
        var response = await _client?.PostAsync("study-groups", content)!;

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Test]
    public async Task CreateStudyGroup_ShortName_ReturnsBadRequest()
    {
        var group = new { Name = "Bad", Subject = "Math", CreateDate = DateTime.UtcNow };
        var content = new StringContent(JsonConvert.SerializeObject(group), Encoding.UTF8, "application/json");

        var response = await _client?.PostAsync("study-groups", content)!;

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}