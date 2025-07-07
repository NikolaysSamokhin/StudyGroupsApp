using System.ComponentModel.DataAnnotations;

namespace StudyGroupsApp.Models;

/// <summary>
/// Represents a user in the study groups application.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    [Key]
    public int Id { get; init; }

    /// <summary>
    /// Gets or sets the name of the user.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string? Name { get; init; }
}