using StudyGroupsApp.enums;
using StudyGroupsApp.Models;

namespace StudyGroupsApp.Repositories;

/// <summary>
/// Repository interface for managing study groups.
/// </summary>
public interface IStudyGroupRepository
{
    /// <summary>
    /// Creates a new study group asynchronously.
    /// </summary>
    /// <param name="studyGroup">The study group to create.</param>
    Task CreateStudyGroupAsync(StudyGroup studyGroup);

    /// <summary>
    /// Gets all study groups asynchronously.
    /// </summary>
    /// <returns>A list of all study groups.</returns>
    Task<List<StudyGroup>> GetStudyGroupsAsync();

    /// <summary>
    /// Searches for study groups by subject asynchronously.
    /// </summary>
    /// <param name="subject">The subject to search for.</param>
    /// <returns>A list of matching study groups.</returns>
    Task<List<StudyGroup>> SearchStudyGroupsAsync(Subject subject);

    /// <summary>
    /// Adds a user to a study group asynchronously.
    /// </summary>
    /// <param name="studyGroupId">The ID of the study group.</param>
    /// <param name="userId">The ID of the user.</param>
    Task JoinStudyGroupAsync(int studyGroupId, int userId);

    /// <summary>
    /// Removes a user from a study group asynchronously.
    /// </summary>
    /// <param name="studyGroupId">The ID of the study group.</param>
    /// <param name="userId">The ID of the user.</param>
    Task LeaveStudyGroupAsync(int studyGroupId, int userId);
}