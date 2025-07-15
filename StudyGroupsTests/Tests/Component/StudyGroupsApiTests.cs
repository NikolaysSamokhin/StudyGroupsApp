using System.Net;
using FluentAssertions;
using FluentAssertions.Execution;
using StudyGroupsApp.enums;
using StudyGroupsApp.Models;

namespace StudyGroupsTests.Tests.Component;

[TestFixture]
public class StudyGroupControllerComponentTests : BaseTest
{
    [Test, TestCaseSource(nameof(ValidSubjects))]
    public async Task CreateStudyGroupShouldReturn200WhenDataIsValidAsyncTest(Subject subject)
    {
        var now = DateTime.UtcNow;

        var mathGroup = new StudyGroup
        {
            Name = $"MathGroup_{Guid.NewGuid():N}"[..25],
            Subject = Subject.Math,
            CreateDate = now,
            Users =
            [
                new User { Id = 1, Name = "Miguel Rodriguez" },
                new User { Id = 2, Name = "Anna Maria" }
            ]
        };

        var chemGroup = new StudyGroup
        {
            Name = $"ChemGroup_{Guid.NewGuid():N}"[..25],
            Subject = Subject.Chemistry,
            CreateDate = now,
            Users =
            [
                new User { Id = 3, Name = "John" },
                new User { Id = 4, Name = "Sophia Lee" }
            ]
        };

        var physGroup = new StudyGroup
        {
            Name = $"PhysGroup_{Guid.NewGuid():N}"[..25],
            Subject = Subject.Physics,
            CreateDate = now,
            Users =
            [
                new User { Id = 5, Name = "Emma Davis" },
                new User { Id = 6, Name = "Liam Smith" }
            ]
        };

        // === Отправка запросов ===
        var mathResponse = await Client.CreateStudyGroupAsync(mathGroup);
        var chemResponse = await Client.CreateStudyGroupAsync(chemGroup);
        var physResponse = await Client.CreateStudyGroupAsync(physGroup);

        var (mathGroups, mathStatus) = await Client.GetAllStudyGroupsAsync(Subject.Math);
        var (chemGroups, chemStatus) = await Client.GetAllStudyGroupsAsync(Subject.Chemistry);
        var (physGroups, physStatus) = await Client.GetAllStudyGroupsAsync(Subject.Physics);

        using (new AssertionScope())
        {
            mathResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            chemResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            physResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            mathStatus.Should().Be(HttpStatusCode.OK);
            chemStatus.Should().Be(HttpStatusCode.OK);
            physStatus.Should().Be(HttpStatusCode.OK);

            var actualMathGroup = mathGroups!.FirstOrDefault(g => g.Name == mathGroup.Name);
            var actualChemGroup = chemGroups!.FirstOrDefault(g => g.Name == chemGroup.Name);
            var actualPhysGroup = physGroups!.FirstOrDefault(g => g.Name == physGroup.Name);

            actualMathGroup.Should().NotBeNull("Math group should exist");
            actualChemGroup.Should().NotBeNull("Chemistry group should exist");
            actualPhysGroup.Should().NotBeNull("Physics group should exist");

            actualMathGroup.Should().BeEquivalentTo(mathGroup, options => options
                .Excluding(g => g.StudyGroupId)
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation,
                    TimeSpan.FromSeconds(5)))
                .WhenTypeIs<DateTime>());

            actualChemGroup.Should().BeEquivalentTo(chemGroup, options => options
                .Excluding(g => g.StudyGroupId)
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation,
                    TimeSpan.FromSeconds(5)))
                .WhenTypeIs<DateTime>());

            actualPhysGroup.Should().BeEquivalentTo(physGroup, options => options
                .Excluding(g => g.StudyGroupId)
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation,
                    TimeSpan.FromSeconds(5)))
                .WhenTypeIs<DateTime>());
        }
    }

    [Test]
    public async Task CreateStudyGroupShouldReturn400WhenNameTooShortAsyncTest()
    {
        var group = new StudyGroup
        {
            Name = "1234",
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow,
            Users = []
        };

        var response = await Client.CreateStudyGroupAsync(group);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task CreateStudyGroupShouldReturn400WhenNameTooLongAsyncTest()
    {
        var group = new StudyGroup
        {
            Name = new string('A', 31),
            Subject = Subject.Physics,
            CreateDate = DateTime.UtcNow,
            Users = []
        };

        var response = await Client.CreateStudyGroupAsync(group);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task JoinStudyGroupShouldReturn404WhenUserNotFoundAsyncTest()
    {
        var group = new StudyGroup
        {
            Name = $"Math_{Guid.NewGuid():N}"[..25],
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow,
            Users = []
        };

        await Client.CreateStudyGroupAsync(group);
        var (groups, _) = await Client.GetAllStudyGroupsAsync();
        var target = groups!.First(g => g.Name == group.Name);

        var response = await Client.JoinStudyGroupAsync(target.StudyGroupId, 9999);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Test]
    public async Task CreateStudyGroupShouldReturn400WhenSubjectInvalidAsyncTest()
    {
        var payload = new
        {
            Name = "Invalid Group",
            Subject = 999,
            CreateDate = DateTime.UtcNow,
            Users = new List<object>()
        };

        var response = await Client.CreateStudyGroupAsync(payload);

        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.Should().Contain("Invalid subject");
        }
    }

    [Test]
    public async Task GetStudyGroupsShouldReturn200AndListAsyncTest()
    {
        var newGroup = new StudyGroup
        {
            Name = $"Group_{Guid.NewGuid():N}"[..25],
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow,
            Users = []
        };

        var createResponse = await Client.CreateStudyGroupAsync(newGroup);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var (groups, statusCode) = await Client.GetAllStudyGroupsAsync();

        using (new AssertionScope())
        {
            statusCode.Should().Be(HttpStatusCode.OK);
            groups.Should().NotBeNull();
            groups.Should().Contain(g => g.Name == newGroup.Name);
        }
    }

    [Test]
    public async Task GetStudyGroupsShouldReturnFilteredBySubjectAndSortedAsyncTest()
    {
        var now = DateTime.UtcNow;

        var group1 = new StudyGroup
        {
            Name = $"Phys_{Guid.NewGuid():N}"[..25],
            Subject = Subject.Physics,
            CreateDate = now.AddMinutes(-2),
            Users = []
        };

        var group2 = new StudyGroup
        {
            Name = $"Chem_{Guid.NewGuid():N}"[..25],
            Subject = Subject.Chemistry,
            CreateDate = now.AddMinutes(-1),
            Users = []
        };

        var group3 = new StudyGroup
        {
            Name = $"Math_{Guid.NewGuid():N}"[..25],
            Subject = Subject.Math,
            CreateDate = now,
            Users = []
        };

        await Client.CreateStudyGroupAsync(group1);
        await Client.CreateStudyGroupAsync(group2);
        await Client.CreateStudyGroupAsync(group3);

        var (groups, statusCode) = await Client.GetAllStudyGroupsAsync(Subject.Chemistry, "desc");

        using (new AssertionScope())
        {
            statusCode.Should().Be(HttpStatusCode.OK);
            groups.Should().OnlyContain(g => g.Subject == Subject.Chemistry);
            groups.Should().BeInDescendingOrder(g => g.CreateDate);
        }
    }

    [Test]
    public async Task JoinStudyGroupShouldReturn200WhenJoiningOneOfMultipleGroupsAsyncTest()
    {
        var user1 = new User { Id = 1, Name = "Miguel" };
        var user2 = new User { Id = 2, Name = "Anna" };
        var user3 = new User { Id = 3, Name = "John" };

        var studyGroup1 = new StudyGroup
        {
            Name = "ChemGroup_" + Guid.NewGuid().ToString("N")[..8],
            Subject = Subject.Chemistry,
            CreateDate = DateTime.UtcNow,
            Users = [user1, user2]
        };

        var studyGroup2 = new StudyGroup
        {
            Name = "MathGroup_" + Guid.NewGuid().ToString("N")[..8],
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow,
            Users = [user3]
        };

        await Client.CreateStudyGroupAsync(studyGroup1);
        await Client.CreateStudyGroupAsync(studyGroup2);

        var (groups, status) = await Client.GetAllStudyGroupsAsync();
        status.Should().Be(HttpStatusCode.OK);

        var targetGroup = groups!.First(g => g.Name == studyGroup2.Name);

        var joinResponse = await Client.JoinStudyGroupAsync(targetGroup.StudyGroupId, user1.Id);
        joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task JoinStudyGroupShouldReturn409WhenAlreadyMemberAsyncTest()
    {
        var user = new User
        {
            Id = 1,
            Name = "Miguel"
        };

        var group = new StudyGroup
        {
            Name = $"MathGroup_{Guid.NewGuid():N}"[..25],
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow,
            Users = [user]
        };

        await Client.CreateStudyGroupAsync(group);
        var (groups, _) = await Client.GetAllStudyGroupsAsync();
        var target = groups!.First(g => g.Name == group.Name);

        await Client.JoinStudyGroupAsync(target.StudyGroupId, user.Id);
        var secondJoin = await Client.JoinStudyGroupAsync(target.StudyGroupId, user.Id);

        using (new AssertionScope())
        {
            secondJoin.StatusCode.Should().Be(HttpStatusCode.Conflict);
            var msg = await secondJoin.Content.ReadAsStringAsync();
            msg.Should().Contain("already");
        }
    }

    [Test]
    public async Task LeaveStudyGroupShouldReturn200WhenUserInGroupAsyncTest()
    {
        var user1 = new User
        {
            Id = 1,
            Name = "Miguel Rodriguez"
        };

        var user2 = new User
        {
            Id = 2,
            Name = "Anna Maria"
        };
        var group = new StudyGroup
        {
            Name = $"ChemGroup_{Guid.NewGuid():N}"[..25],
            Subject = Subject.Chemistry,
            CreateDate = DateTime.UtcNow,
            Users = [user1, user2]
        };

        await Client.CreateStudyGroupAsync(group);
        var (groups, _) = await Client.GetAllStudyGroupsAsync();
        var target = groups!.First(g => g.Name == group.Name);

        var leaveResponse = await Client.LeaveStudyGroupAsync(target.StudyGroupId, user1.Id);

        leaveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task LeaveStudyGroupShouldReturn404WhenUserNotInGroupAsyncTest()
    {
        const int userId = 999;

        var group = new StudyGroup
        {
            Name = $"PhysGroup_{Guid.NewGuid():N}"[..25],
            Subject = Subject.Physics,
            CreateDate = DateTime.UtcNow,
            Users = []
        };

        await Client.CreateStudyGroupAsync(group);
        var (groups, _) = await Client.GetAllStudyGroupsAsync();
        var target = groups!.First(g => g.Name == group.Name);

        var leaveResponse = await Client.LeaveStudyGroupAsync(target.StudyGroupId, userId);

        leaveResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task DeleteAllStudyGroupsShouldReturn204WhenEmptyAsyncTest()
    {
        await Client.DeleteAllStudyGroups();
        var response = await Client.DeleteAllStudyGroups();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task CreateStudyGroupWithDuplicateSubjectShouldReturn400AsyncTest()
    {
        var group1 = new StudyGroup
        {
            Name = "Group1",
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow,
            Users = []
        };

        var group2 = new StudyGroup
        {
            Name = "Group2",
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow,
            Users = []
        };

        await Client.CreateStudyGroupAsync(group1);
        var response = await Client.CreateStudyGroupAsync(group2);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateStudyGroupWithSameNameDifferentSubjectShouldReturn200AsyncTest()
    {
        var name = $"Group_{Guid.NewGuid():N}"[..25];

        var group1 = new StudyGroup
        {
            Name = name,
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow,
            Users = []
        };

        var group2 = new StudyGroup
        {
            Name = name,
            Subject = Subject.Chemistry,
            CreateDate = DateTime.UtcNow,
            Users = []
        };

        var response1 = await Client.CreateStudyGroupAsync(group1);
        var response2 = await Client.CreateStudyGroupAsync(group2);

        using (new AssertionScope())
        {
            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Test]
    public async Task DeleteAllStudyGroupsShouldReturn204AndCleanUpAsyncTest()
    {
        var response = await Client.DeleteAllStudyGroups();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var (groups, status) = await Client.GetAllStudyGroupsAsync();
        status.Should().Be(HttpStatusCode.NotFound);
        groups.Should().BeNullOrEmpty("all study groups should be deleted");
    }
}