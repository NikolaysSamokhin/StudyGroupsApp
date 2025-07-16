using Microsoft.EntityFrameworkCore;
using StudyGroupsApp.Data;
using StudyGroupsApp.enums;
using StudyGroupsApp.Models;

namespace StudyGroupsApp.Repositories;

/// <summary>
/// Repository for managing study groups.
/// </summary>
public class StudyGroupRepositoryUnit(AppDbContext? context) : IStudyGroupRepository
{
    /// <summary>
    /// Creates a new study group asynchronously.
    /// </summary>
    /// <param name="studyGroup">Study group to create.</param>
    /// <exception cref="ArgumentException">If subject is invalid.</exception>
    /// <exception cref="InvalidOperationException">If group with same subject exists.</exception>
    public async Task CreateStudyGroupAsync(StudyGroup studyGroup)
    {
        if (!Enum.IsDefined(studyGroup.Subject))
            throw new ArgumentException("Invalid subject value.", nameof(studyGroup.Subject));

        var exists = await context?.StudyGroups.AnyAsync(g => g.Subject == studyGroup.Subject)!;
        if (exists)
            throw new InvalidOperationException("A study group with the same subject already exists.");

        var userIds = studyGroup.Users.Select(u => u.Id).ToList();

        var existingUsers = await context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();


        var existingIds = existingUsers.Select(u => u.Id).ToHashSet();
        var missingUsers = studyGroup.Users
            .Where(u => !existingIds.Contains(u.Id))
            .ToList();

        if (missingUsers.Count > 0)
        {
            context.Users.AddRange(missingUsers);
            await context.SaveChangesAsync();  
            existingUsers.AddRange(missingUsers);
        }

        studyGroup.Users = existingUsers;

        context.StudyGroups.Add(studyGroup);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets all study groups with their users asynchronously.
    /// </summary>
    /// <returns>List of all study groups.</returns>
    /// <exception cref="InvalidOperationException">If no groups exist.</exception>
    public async Task<List<StudyGroup>> GetStudyGroupsAsync()
    {
        var groups = await context!.StudyGroups.Include(sg => sg.Users).ToListAsync();
        if (groups == null || groups.Count == 0)
            throw new InvalidOperationException("No study groups have been created.");
        return groups;
    }

    /// <summary>
    /// Asynchronously searches for study groups by subject.
    /// </summary>
    /// <param name="subject">Subject to filter by.</param>
    /// <returns>List of matching study groups.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no study groups are found for the given subject.</exception>
    public async Task<List<StudyGroup>> SearchStudyGroupsAsync(Subject subject)
    {
        var groups = await context!.StudyGroups
            .Where(sg => sg.Subject == subject)
            .Include(sg => sg.Users)
            .ToListAsync();

        if (groups == null || groups.Count == 0)
            throw new InvalidOperationException("No study groups found for the specified subject.");

        return groups;
    }

    /// <summary>
    /// Adds a user to a study group asynchronously.
    /// </summary>
    /// <param name="studyGroupId">Study group ID.</param>
    /// <param name="userId">User ID.</param>
    /// <exception cref="InvalidOperationException">
    /// If group or user not found, or user already in group.
    /// </exception>
    public async Task JoinStudyGroupAsync(int studyGroupId, int userId)
    {
        var group = await context!.StudyGroups.Include(sg => sg.Users)
            .FirstOrDefaultAsync(sg => sg.StudyGroupId == studyGroupId);
        if (group == null)
            throw new InvalidOperationException("Study group not found.");

        var user = await context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (group.Users.Any(u => u.Id == userId))
            throw new InvalidOperationException("User is already a member of the study group.");

        group.Users.Add(user);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Removes a user from a study group asynchronously.
    /// </summary>
    /// <param name="studyGroupId">Study group ID.</param>
    /// <param name="userId">User ID.</param>
    /// <exception cref="InvalidOperationException">
    /// If group or user not found, or user not in group.
    /// </exception>
    public async Task LeaveStudyGroupAsync(int studyGroupId, int userId)
    {
        var group = await context!.StudyGroups.Include(sg => sg.Users)
            .FirstOrDefaultAsync(sg => sg.StudyGroupId == studyGroupId);
        if (group == null)
            throw new InvalidOperationException("Study group not found.");

        var user = await context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (group.Users.All(u => u.Id != userId))
            throw new InvalidOperationException("User is not a member of the study group.");

        group.Users.Remove(user);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes all study groups asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public async Task DeleteAllStudyGroupsAsync()
    {
        context!.StudyGroups.RemoveRange(context.StudyGroups);
        await context.SaveChangesAsync();
    }
}