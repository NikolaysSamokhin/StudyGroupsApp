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
    /// The study group to create. Required fields: Name (5-30 chars), Subject (Math, Chemistry, Physics), CreateDate.
    /// </param>
    /// <returns>
    /// 200 OK on success, 400 Bad Request on validation or operation error.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> CreateStudyGroup([FromBody] StudyGroup studyGroup)
    {
        try
        {
            await studyGroupRepository.CreateStudyGroupAsync(studyGroup);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets a list of study groups, optionally filtered by subject and sorted by creation date.
    /// </summary>
    /// <param name="subject">Optional subject filter (Math, Chemistry, Physics).</param>
    /// <param name="sort">Sort order: "asc" (default) or "desc" by creation date.</param>
    /// <returns>
    /// 200 OK with a list of study groups, 404 Not Found if no groups found for the subject.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetStudyGroups(
        [FromQuery] Subject? subject,
        [FromQuery] string? sort = "asc")
    {
        try
        {
            var groups = subject.HasValue
                ? await studyGroupRepository.SearchStudyGroupsAsync(subject.Value)
                : await studyGroupRepository.GetStudyGroupsAsync();

            groups = sort?.ToLower() == "desc"
                ? groups.OrderByDescending(g => g.CreateDate).ToList()
                : groups.OrderBy(g => g.CreateDate).ToList();

            return Ok(groups);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Adds a user to a study group.
    /// </summary>
    /// <param name="id">Study group ID.</param>
    /// <param name="userId">User ID to add.</param>
    /// <returns>
    /// 200 OK on success, 409 Conflict if user is already a member.
    /// </returns>
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
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Removes a user from a study group.
    /// </summary>
    /// <param name="id">Study group ID.</param>
    /// <param name="userId">User ID to remove.</param>
    /// <returns>
    /// 200 OK on success, 404 Not Found if group or user not found or user not a member.
    /// </returns>
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
            return NotFound(ex.Message);
        }
    }
    
    
    /// <summary>
    /// Deletes all study groups.
    /// </summary>
    /// <returns>204 No Content.</returns>
    [HttpDelete]
    public async Task<IActionResult> DeleteAllStudyGroups()
    {
        await studyGroupRepository.DeleteAllStudyGroupsAsync();
        return NoContent();
    }
}