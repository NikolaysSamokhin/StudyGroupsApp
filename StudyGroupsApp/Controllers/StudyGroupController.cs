using Microsoft.AspNetCore.Mvc;
using StudyGroupsApp.Models;
using StudyGroupsApp.Repositories;
using StudyGroupsApp.enums;

namespace StudyGroupsApp.Controllers;

[ApiController]
[Route("study-groups")]
public class StudyGroupController(IStudyGroupRepository studyGroupRepository, ILogger<StudyGroupController> logger) : ControllerBase
{
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
