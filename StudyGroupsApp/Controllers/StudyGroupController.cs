using Microsoft.AspNetCore.Mvc;
using StudyGroupsApp.Models;
using StudyGroupsApp.Repositories;
using StudyGroupsApp.enums;

namespace StudyGroupsApp.Controllers;

/// <summary>
/// API controller for managing study groups.  
/// Provides endpoints to create, retrieve, join, and leave study groups.  
/// </summary>
[ApiController]
[Route("study-groups")]
public class StudyGroupController(IStudyGroupRepository studyGroupRepository) : ControllerBase
{
    /// <summary>
    /// Creates a new study group.
    /// </summary>
    /// <param name="studyGroup">
    /// The study group to be created.  
    /// Required fields: Name (5-30 characters), Subject (Math, Chemistry, or Physics), and CreateDate.
    /// </param>
    /// <returns>
    /// Returns <see cref="OkResult"/> (200) on success.  
    /// May return 400 if input validation is added in the future.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> CreateStudyGroup([FromBody] StudyGroup studyGroup)
    {
        await studyGroupRepository.CreateStudyGroupAsync(studyGroup);
        return Ok();
    }

    /// <summary>
    /// Retrieves a list of study groups, optionally filtered by subject and sorted by creation date.
    /// </summary>
    /// <param name="subject">Optional subject filter (Math, Chemistry, or Physics).</param>
    /// <param name="sort">Optional sort order: "asc" (default) or "desc" based on creation date.</param>
    /// <returns>
    /// A list of <see cref="StudyGroup"/> objects matching the filter and sort criteria.  
    /// Returns 200 OK even if the list is empty.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetStudyGroups(
        [FromQuery] Subject? subject,
        [FromQuery] string? sort = "asc")
    {
        var groups = subject.HasValue
            ? await studyGroupRepository.SearchStudyGroupsAsync(subject.Value)
            : await studyGroupRepository.GetStudyGroupsAsync();

        groups = sort?.ToLower() == "desc" ? groups.OrderByDescending(g => g.CreateDate).ToList()
            : groups.OrderBy(g => g.CreateDate).ToList();

        return Ok(groups);
    }

    /// <summary>
    /// Adds a user to the specified study group.
    /// </summary>
    /// <param name="id">The ID of the study group.</param>
    /// <param name="userId">The ID of the user who is joining.</param>
    /// <returns>
    /// Returns <see cref="OkResult"/> (200) on success.  
    /// No error returned if user already joined or doesn't exist.
    /// </returns>
    [HttpPost("{id:int}/join")]
    public async Task<IActionResult> JoinStudyGroup(int id, [FromQuery] int userId)
    {
        await studyGroupRepository.JoinStudyGroupAsync(id, userId);
        return Ok();
    }

    /// <summary>
    /// Removes a user from the specified study group.
    /// </summary>
    /// <param name="id">The ID of the study group.</param>
    /// <param name="userId">The ID of the user to remove.</param>
    /// <returns>
    /// Returns <see cref="OkResult"/> (200) on success.  
    /// No error returned if user was not part of the group.
    /// </returns>
    [HttpDelete("{id:int}/leave")]
    public async Task<IActionResult> LeaveStudyGroup(int id, [FromQuery] int userId)
    {
        await studyGroupRepository.LeaveStudyGroupAsync(id, userId);
        return Ok();
    }
}
