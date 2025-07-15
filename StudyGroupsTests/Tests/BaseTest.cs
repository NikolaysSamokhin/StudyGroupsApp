using System.Net;
using FluentAssertions;
using StudyGroupsApp.enums;
using StudyGroupsApp.Models;
using StudyGroupsApp.Tests.Client;

namespace StudyGroupsTests.Tests;

public class BaseTest
{
    protected StudyGroupApiClient Client = null!;


    protected static readonly Subject[] ValidSubjects =
    [
        Subject.Math,
        Subject.Chemistry,
        Subject.Physics
    ];
    
    protected static List<StudyGroup> ValidStudyGroups =>
    [
        new StudyGroup
        {
            Name = "Chemistry Group A",
            Subject = Subject.Chemistry,
            CreateDate = DateTime.UtcNow.AddMinutes(-2),
            Users =
            [
                new User { Id = 1, Name = "Miguel" },
                new User { Id = 2, Name = "Anna" }
            ]
        },

        new StudyGroup
        {
            Name = "Math Group B",
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow.AddMinutes(-1),
            Users = [new User { Id = 3, Name = "John" }]
        }
    ];
    
    [SetUp]
    public async Task Setup()
    {
        Client = new StudyGroupApiClient("http://localhost:5000");

        var response = await Client.DeleteAllStudyGroups();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        var response = await Client.DeleteAllStudyGroups();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}