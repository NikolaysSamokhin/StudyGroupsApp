using Microsoft.EntityFrameworkCore;
using StudyGroupsApp.Data;
using StudyGroupsApp.enums;
using StudyGroupsApp.Models;
using StudyGroupsApp.Repositories;

namespace ComponentTests.Tests;

public class StudyGroupRepositoryTests
{
    private AppDbContext? _context;
    private StudyGroupRepository? _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new StudyGroupRepository(_context);
    }

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

        Assert.That(await _context!.StudyGroups.CountAsync(), Is.EqualTo(1));
    }

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

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _repository!.CreateStudyGroupAsync(group));
    }

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
            CreateDate = DateTime.UtcNow, Users = []
        };

        await _repository!.CreateStudyGroupAsync(group1);
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository.CreateStudyGroupAsync(group2));
    }

    public void GetStudyGroupsWhenEmptyThrowsTest()
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository!.GetStudyGroupsAsync());
    }

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
        Assert.That(result.Count, Is.EqualTo(1));
    }

    public async Task SearchStudyGroupsBySubjectReturnsCorrectAsyncTest()
    {
        _context!.StudyGroups.Add(new StudyGroup
            { Name = "MathG",
                Subject = Subject.Math,
                CreateDate = DateTime.UtcNow,
                Users = []
            });
        _context.StudyGroups.Add(new StudyGroup
            { Name = "ChemG",
                Subject = Subject.Chemistry,
                CreateDate = DateTime.UtcNow,
                Users = []
            });
        await _context.SaveChangesAsync();

        var result = await _repository!.SearchStudyGroupsAsync(Subject.Chemistry);
        Assert.That(result.All(g => g.Subject == Subject.Chemistry));
    }

    public void SearchStudyGroupsWithNoMatchesThrowsTest()
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository!.SearchStudyGroupsAsync(Subject.Physics));
    }

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
        var updated = await _context.StudyGroups.Include(g => 
            g.Users).FirstAsync(g => g.StudyGroupId == 3);

        Assert.That(updated.Users.Any(u => u.Id == 10));
    }

    public void JoinStudyGroupWithNonexistentGroupThrowsTest()
    {
        _context!.Users.Add(new User { Id = 11, Name = "Tom" });
        _context.SaveChanges();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository!.JoinStudyGroupAsync(999, 11));
    }

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

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository!.JoinStudyGroupAsync(4, 999));
    }

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

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository!.JoinStudyGroupAsync(5, 13));
    }

    public async Task LeaveStudyGroupWithValidDataRemovesUserAsyncTest()
    {
        var user = new User { Id = 14, Name = "Sara" };
        var group = new StudyGroup
        {
            StudyGroupId = 6, Name = "Group2", Subject = Subject.Math, CreateDate = DateTime.UtcNow,
            Users = [user]
        };

        _context!.Users.Add(user);
        _context.StudyGroups.Add(group);
        await _context.SaveChangesAsync();

        await _repository!.LeaveStudyGroupAsync(6, 14);
        var updated = await _context.StudyGroups.Include(g => g.Users).
            FirstAsync(g => g.StudyGroupId == 6);

        Assert.That(updated.Users.Any(u => u.Id == 14), Is.False);
    }

    public async Task LeaveStudyGroupWithUserNotInGroupThrowsAsyncTest()
    {
        var user = new User
        {
            Id = 15, 
            Name = "Jon"
        };
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

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository!.LeaveStudyGroupAsync(7, 15));
    }

    public void LeaveStudyGroupWithNonexistentGroupThrowsTest()
    {
        _context!.Users.Add(new User { Id = 16, Name = "Ghost" });
        _context.SaveChanges();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository!.LeaveStudyGroupAsync(999, 16));
    }

    public async Task LeaveStudyGroupWithNonexistentUserThrowsAsyncTest()
    {
        var group = new StudyGroup
        {
            StudyGroupId = 8, Name = "SoloGroup", Subject = Subject.Math, CreateDate = DateTime.UtcNow,
            Users = []
        };
        _context!.StudyGroups.Add(group);
        await _context.SaveChangesAsync();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository!.LeaveStudyGroupAsync(8, 999));
    }

    public async Task DeleteAllStudyGroupsRemovesAllAsyncTest()
    {
        _context!.StudyGroups.Add(new StudyGroup
            { Name = "A", Subject = Subject.Math, CreateDate = DateTime.UtcNow, Users = [] });
        _context!.StudyGroups.Add(new StudyGroup(name: "B", subject: Subject.Chemistry, createDate: DateTime.UtcNow,
            users: new List<User>()));
        await _context.SaveChangesAsync();

        await _repository!.DeleteAllStudyGroupsAsync();

        Assert.That(await _context.StudyGroups.CountAsync(), Is.EqualTo(0));
    }
}