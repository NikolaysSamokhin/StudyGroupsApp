using Microsoft.AspNetCore.Mvc;
using StudyGroupsApp.Models;
using StudyGroupsApp.Repositories;
using StudyGroupsApp.enums;

namespace StudyGroupsApp.Controllers;

/// <summary>
/// Controller for managing study groups.
/// </summary>
[ApiController]
[Route("study-groups")]
public class StudyGroupController(IStudyGroupRepository studyGroupRepository) : ControllerBase
{
    /// <summary>
    /// Creates a new study group.
    /// </summary>
    /// <param name="studyGroup">The study group to create.</param>
    /// <returns>Action result indicating the outcome of the operation.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateStudyGroup([FromBody] StudyGroup studyGroup)
    {
        await studyGroupRepository.CreateStudyGroupAsync(studyGroup);
        return Ok();
    }

    /// <summary>
    /// Gets a list of all study groups with optional filtering by subject and sorting.
    /// Example: /study-groups?subject=Math&amp;sort=desc
    /// </summary>
    /// <param name="subject">Optional subject to filter study groups.</param>
    /// <param name="sort">Sort order: "asc" or "desc". Default is "asc".</param>
    /// <returns>List of study groups.</returns>
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
    /// Adds a user to a study group.
    /// </summary>
    /// <param name="id">The ID of the study group.</param>
    /// <param name="userId">The ID of the user to add.</param>
    /// <returns>Action result indicating the outcome of the operation.</returns>
    [HttpPost("{id:int}/join")]
    public async Task<IActionResult> JoinStudyGroup(int id, [FromQuery] int userId)
    {
        await studyGroupRepository.JoinStudyGroupAsync(id, userId);
        return Ok();
    }

    /// <summary>
    /// Removes a user from a study group.
    /// </summary>
    /// <param name="id">The ID of the study group.</param>
    /// <param name="userId">The ID of the user to remove.</param>
    /// <returns>Action result indicating the outcome of the operation.</returns>
    [HttpDelete("{id:int}/leave")]
    public async Task<IActionResult> LeaveStudyGroup(int id, [FromQuery] int userId)
    {
        await studyGroupRepository.LeaveStudyGroupAsync(id, userId);
        return Ok();
    }
}