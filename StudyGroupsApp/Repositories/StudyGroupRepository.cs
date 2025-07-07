using Microsoft.EntityFrameworkCore;
using StudyGroupsApp.Data;
using StudyGroupsApp.enums;
using StudyGroupsApp.Models;

namespace StudyGroupsApp.Repositories;

/// <summary>
/// Repository for managing study groups in the application.
/// </summary>
public class StudyGroupRepository(AppDbContext context) : IStudyGroupRepository
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Asynchronously creates a new study group.
    /// </summary>
    /// <param name="studyGroup">The study group to create.</param>
    public async Task CreateStudyGroupAsync(StudyGroup studyGroup)
    {
        _context.StudyGroups.Add(studyGroup);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Asynchronously retrieves all study groups with their users.
    /// </summary>
    /// <returns>A list of all study groups.</returns>
    public async Task<List<StudyGroup>> GetStudyGroupsAsync()
    {
        return await _context.StudyGroups.Include(sg => sg.Users).ToListAsync();
    }

    /// <summary>
    /// Asynchronously searches for study groups by subject.
    /// </summary>
    /// <param name="subject">The subject to search for.</param>
    /// <returns>A list of matching study groups.</returns>
    public async Task<List<StudyGroup>> SearchStudyGroupsAsync(Subject subject)
    {
        return await _context.StudyGroups
            .Where(sg => sg.Subject == subject)
            .Include(sg => sg.Users)
            .ToListAsync();
    }

    /// <summary>
    /// Asynchronously adds a user to a study group.
    /// </summary>
    /// <param name="studyGroupId">The ID of the study group.</param>
    /// <param name="userId">The ID of the user.</param>
    public async Task JoinStudyGroupAsync(int studyGroupId, int userId)
    {
        var group = await _context.StudyGroups.Include(sg => sg.Users)
            .FirstOrDefaultAsync(sg => sg.StudyGroupId == studyGroupId);
        var user = await _context.Users.FindAsync(userId);
        if (group != null && user != null && group.Users.All(u => u.Id != userId))
        {
            group.Users.Add(user);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Asynchronously removes a user from a study group.
    /// </summary>
    /// <param name="studyGroupId">The ID of the study group.</param>
    /// <param name="userId">The ID of the user.</param>
    public async Task LeaveStudyGroupAsync(int studyGroupId, int userId)
    {
        var group = await _context.StudyGroups.Include(sg => sg.Users)
            .FirstOrDefaultAsync(sg => sg.StudyGroupId == studyGroupId);
        var user = group?.Users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            group?.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}