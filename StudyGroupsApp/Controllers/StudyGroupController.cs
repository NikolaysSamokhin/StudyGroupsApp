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
public class StudyGroupController(IStudyGroupRepository studyGroupRepository, ILogger<StudyGroupController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new study group.
    /// </summary>
    /// <param name="studyGroup">The study group to create.</param>
    /// <returns>Action result indicating the outcome of the operation.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateStudyGroupAsync([FromBody] StudyGroup studyGroup)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await studyGroupRepository.CreateStudyGroupAsync(studyGroup);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Validation failed on CreateStudyGroup");
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation on CreateStudyGroup");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in CreateStudyGroup");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets a list of study groups, optionally filtered by subject and sorted by creation date.
    /// </summary>
    /// <param name="subject">Optional subject to filter study groups.</param>
    /// <param name="sort">Sort order (ascending or descending).</param>
    /// <returns>List of study groups.</returns>
    [HttpGet]
    public async Task<IActionResult> GetStudyGroups(
        [FromQuery] Subject? subject,
        [FromQuery] SortOrder sort = SortOrder.Asc)
    {
        try
        {
            var groups = subject.HasValue
                ? await studyGroupRepository.SearchStudyGroupsAsync(subject.Value)
                : await studyGroupRepository.GetStudyGroupsAsync();

            groups = sort == SortOrder.Desc
                ? groups.OrderByDescending(g => g.CreateDate).ToList()
                : groups.OrderBy(g => g.CreateDate).ToList();

            return Ok(groups);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "No study groups found");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in GetStudyGroups");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Adds a user to a study group.
    /// </summary>
    /// <param name="id">Study group ID.</param>
    /// <param name="userId">User ID.</param>
    /// <returns>Action result indicating the outcome of the operation.</returns>
    [HttpPost("{id:int}/join")]
    public async Task<IActionResult> JoinStudyGroup(int id, [FromQuery] int userId)
    {
        try
        {
            await studyGroupRepository.JoinStudyGroupAsync(id, userId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "JoinStudyGroup failed");
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in JoinStudyGroup");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Removes a user from a study group.
    /// </summary>
    /// <param name="id">Study group ID.</param>
    /// <param name="userId">User ID.</param>
    /// <returns>Action result indicating the outcome of the operation.</returns>
    [HttpDelete("{id:int}/leave")]
    public async Task<IActionResult> LeaveStudyGroup(int id, [FromQuery] int userId)
    {
        try
        {
            await studyGroupRepository.LeaveStudyGroupAsync(id, userId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "LeaveStudyGroup failed");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in LeaveStudyGroup");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Deletes all study groups.
    /// </summary>
    /// <returns>No content result.</returns>
    [HttpDelete]
    public async Task<IActionResult> DeleteAllStudyGroups()
    {
        try
        {
            await studyGroupRepository.DeleteAllStudyGroupsAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in DeleteAllStudyGroups");
            return StatusCode(500, "Internal server error");
        }
    }
}