using StudyGroupsApp.enums;
using StudyGroupsApp.Models;

namespace StudyGroupsApp.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        // Apply pending migrations or ensure DB is created
        context.Database.EnsureCreated();

        // Skip if already seeded
        if (context.StudyGroups.Any())
            return;

        // Create users
        var users = new List<User>
        {
            new User { Name = "Alice" },
            new User { Name = "Bob" },
            new User { Name = "Charlie" },
            new User { Name = "Diana" },
            new User { Name = "Ethan" },
            new User { Name = "Fiona" },
            new User { Name = "George" },
            new User { Name = "Helen" }
        };

        context.Users.AddRange(users);
        context.SaveChanges();

        // Create study groups
        var group1 = new StudyGroup
        {
            Name = "Algebra Team",
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow,
            Users = [users[0], users[1], users[4]]
        };

        var group2 = new StudyGroup
        {
            Name = "Organic Chemistry Circle",
            Subject = Subject.Chemistry,
            CreateDate = DateTime.UtcNow,
            Users = [users[2], users[5]]
        };

        var group3 = new StudyGroup
        {
            Name = "Physics Mechanics",
            Subject = Subject.Physics,
            CreateDate = DateTime.UtcNow,
            Users = [users[3], users[4], users[6]]
        };

        var group4 = new StudyGroup
        {
            Name = "Quantum Study Group",
            Subject = Subject.Physics,
            CreateDate = DateTime.UtcNow,
            Users = [users[1], users[5], users[7]]
        };

        var group5 = new StudyGroup
        {
            Name = "Math Olympiad Prep",
            Subject = Subject.Math,
            CreateDate = DateTime.UtcNow,
            Users = [users[0], users[2], users[6]]
        };

        var group6 = new StudyGroup
        {
            Name = "Inorganic Chemistry Crew",
            Subject = Subject.Chemistry,
            CreateDate = DateTime.UtcNow,
            Users = [users[3], users[7]]
        };

        context.StudyGroups.AddRange(group1, group2, group3, group4, group5, group6);
        context.SaveChanges();
    }

}