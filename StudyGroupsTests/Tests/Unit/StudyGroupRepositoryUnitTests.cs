using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyGroupsApp.Data;
using StudyGroupsApp.enums;
using StudyGroupsApp.Models;
using StudyGroupsApp.Repositories;

namespace StudyGroupsTests.Tests.Unit;

[TestFixture]
public class StudyGroupRepositoryUnitTests
{
    private AppDbContext? _context;
    private StudyGroupRepositoryUnit? _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new StudyGroupRepositoryUnit(_context);
    }

    [Test]
    public async Task CreateStudyGroupWithValidDataSavesSuccessfullyAsyncTest()
    {
        var group = new StudyGroup
        {
            StudyGroupId = 1,
            Name = "Math Group",
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow,
            Users = []
        };

        await _repository!.CreateStudyGroupAsync(group);

        var count = await _context!.StudyGroups.CountAsync();
        count.Should().Be(1);
    }

    [Test]
    public void CreateStudyGroupWithInvalidSubjectThrowsTest()
    {
        var group = new StudyGroup
        {
            StudyGroupId = 2,
            Name = "Invalid Subject Group",
            Subject = (Subject)999,
            CreateDate = DateTime.UtcNow,
            Users = []
        };

        var act = async () => await _repository!.CreateStudyGroupAsync(group);
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task CreateStudyGroupWithDuplicateSubjectThrowsAsyncTest()
    {
        var group1 = new StudyGroup
        {
            Name = "Chem1",
            Subject = Subject.Chemistry,
            CreateDate = DateTime.UtcNow,
            Users = []
        };
        var group2 = new StudyGroup
        {
            Name = "Chem2",
            Subject = Subject.Chemistry,
            CreateDate = DateTime.UtcNow,
            Users = []
        };

        await _repository!.CreateStudyGroupAsync(group1);
        var act = async () => await _repository.CreateStudyGroupAsync(group2);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public void GetStudyGroupsWhenEmptyThrowsTest()
    {
        Func<Task> act = async () => await _repository!.GetStudyGroupsAsync();
        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task GetStudyGroupsReturnsAllAsyncTest()
    {
        var group = new StudyGroup
        {
            Name = "PhysicsGroup",
            Subject = Subject.Physics,
            CreateDate = DateTime.UtcNow,
            Users = []
        };
        _context!.StudyGroups.Add(group);
        await _context.SaveChangesAsync();

        var result = await _repository!.GetStudyGroupsAsync();
        result.Should().HaveCount(1);
    }

    [Test]
    public async Task SearchStudyGroupsBySubjectReturnsCorrectAsyncTest()
    {
        _context!.StudyGroups.AddRange(
            new StudyGroup
            {
                Name = "MathG",
                Subject = Subject.Math,
                CreateDate = DateTime.UtcNow,
                Users = []
            },
            new StudyGroup
            {
                Name = "ChemG",
                Subject = Subject.Chemistry,
                CreateDate = DateTime.UtcNow,
                Users = []
            }
        );
        await _context.SaveChangesAsync();

        var result = await _repository!.SearchStudyGroupsAsync(Subject.Chemistry);
        result.Should().OnlyContain(g => g.Subject == Subject.Chemistry);
    }

    [Test]
    public void SearchStudyGroupsWithNoMatchesThrowsTest()
    {
        var act = async () => await _repository!.SearchStudyGroupsAsync(Subject.Physics);
        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task JoinStudyGroupWithValidDataAddsUserAsyncTest()
    {
        var user = new User { Id = 10, Name = "Mike" };
        var group = new StudyGroup
        {
            StudyGroupId = 3,
            Name = "Group1",
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow,
            Users = []
        };

        _context!.Users.Add(user);
        _context.StudyGroups.Add(group);
        await _context.SaveChangesAsync();

        await _repository!.JoinStudyGroupAsync(3, 10);
        var updated = await _context.StudyGroups.Include(g => g.Users)
            .FirstAsync(g => g.StudyGroupId == 3);

        updated.Users.Should().Contain(u => u.Id == 10);
    }

    [Test]
    public void JoinStudyGroupWithNonexistentGroupThrowsTest()
    {
        _context!.Users.Add(new User { Id = 11, Name = "Tom" });
        _context.SaveChanges();

        var act = async () => await _repository!.JoinStudyGroupAsync(999, 11);
        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task JoinStudyGroupWithNonexistentUserThrowsAsyncTest()
    {
        var group = new StudyGroup
        {
            StudyGroupId = 4,
            Name = "Chem",
            Subject = Subject.Chemistry,
            CreateDate = DateTime.UtcNow,
            Users = []
        };
        _context!.StudyGroups.Add(group);
        await _context.SaveChangesAsync();

        var act = async () => await _repository!.JoinStudyGroupAsync(4, 999);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task JoinStudyGroupWithUserAlreadyInThrowsAsyncTest()
    {
        var user = new User { Id = 13, Name = "Leo" };
        var group = new StudyGroup
        {
            StudyGroupId = 5,
            Name = "Physics",
            Subject = Subject.Physics,
            CreateDate = DateTime.UtcNow,
            Users = [user]
        };

        _context!.Users.Add(user);
        _context.StudyGroups.Add(group);
        await _context.SaveChangesAsync();

        Func<Task> act = async () => await _repository!.JoinStudyGroupAsync(5, 13);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task LeaveStudyGroupWithValidDataRemovesUserAsyncTest()
    {
        var user = new User { Id = 14, Name = "Sara" };
        var group = new StudyGroup
        {
            StudyGroupId = 6,
            Name = "Group2",
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow,
            Users = [user]
        };

        _context!.Users.Add(user);
        _context.StudyGroups.Add(group);
        await _context.SaveChangesAsync();

        await _repository!.LeaveStudyGroupAsync(6, 14);
        var updated = await _context.StudyGroups.Include(g => g.Users)
            .FirstAsync(g => g.StudyGroupId == 6);

        updated.Users.Should().NotContain(u => u.Id == 14);
    }

    [Test]
    public async Task LeaveStudyGroupWithUserNotInGroupThrowsAsyncTest()
    {
        var user = new User { Id = 15, Name = "Jon" };
        var group = new StudyGroup
        {
            StudyGroupId = 7,
            Name = "EmptyGroup",
            Subject = Subject.Physics,
            CreateDate = DateTime.UtcNow,
            Users = []
        };

        _context!.Users.Add(user);
        _context.StudyGroups.Add(group);
        await _context.SaveChangesAsync();

        var act = async () => await _repository!.LeaveStudyGroupAsync(7, 15);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public void LeaveStudyGroupWithNonexistentGroupThrowsTest()
    {
        _context!.Users.Add(new User { Id = 16, Name = "Ghost" });
        _context.SaveChanges();

        var act = async () => await _repository!.LeaveStudyGroupAsync(999, 16);
        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task LeaveStudyGroupWithNonexistentUserThrowsAsyncTest()
    {
        var group = new StudyGroup
        {
            StudyGroupId = 8,
            Name = "SoloGroup",
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow,
            Users = []
        };
        _context!.StudyGroups.Add(group);
        await _context.SaveChangesAsync();

        var act = async () => await _repository!.LeaveStudyGroupAsync(8, 999);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task DeleteAllStudyGroupsRemovesAllAsyncTest()
    {
        _context!.StudyGroups.AddRange(
            new StudyGroup { Name = "A", Subject = Subject.Math, CreateDate = DateTime.UtcNow, Users = [] },
            new StudyGroup { Name = "B", Subject = Subject.Chemistry, CreateDate = DateTime.UtcNow, Users = [] }
        );
        await _context.SaveChangesAsync();

        await _repository!.DeleteAllStudyGroupsAsync();

        var count = await _context.StudyGroups.CountAsync();
        count.Should().Be(0);
    }
    
    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }
}
