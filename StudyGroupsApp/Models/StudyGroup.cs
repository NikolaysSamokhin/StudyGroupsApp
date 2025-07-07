using System.ComponentModel.DataAnnotations;
using StudyGroupsApp.enums;

namespace StudyGroupsApp.Models;

/// <summary>
/// Represents a study group with a subject, creation date, and associated users.
/// </summary>
public class StudyGroup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StudyGroup"/> class. Required by EF Core.
    /// </summary>
    public StudyGroup() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="StudyGroup"/> class with specified parameters.
    /// </summary>
    /// <param name="studyGroupId">The unique identifier of the study group.</param>
    /// <param name="name">The name of the study group.</param>
    /// <param name="subject">The subject of the study group.</param>
    /// <param name="createDate">The creation date of the study group.</param>
    /// <param name="users">The list of users in the study group.</param>
    /// <exception cref="ArgumentException">Thrown if the name is invalid.</exception>
    public StudyGroup(int studyGroupId, string name, Subject subject, DateTime createDate, List<User> users)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 5 || name.Length > 30)
            throw new ArgumentException("Name must be between 5 and 30 characters.");

        StudyGroupId = studyGroupId;
        Name = name;
        Subject = subject;
        CreateDate = createDate;
        Users = users ?? new List<User>();
    }

    /// <summary>
    /// Gets or sets the unique identifier of the study group.
    /// </summary>
    [Key]
    public int StudyGroupId { get; set; }

    /// <summary>
    /// Gets or sets the name of the study group.
    /// </summary>
    [Required]
    [StringLength(30, MinimumLength = 5)]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the subject of the study group.
    /// </summary>
    [Required]
    public Subject Subject { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the study group.
    /// </summary>
    [Required]
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Gets the list of users in the study group.
    /// </summary>
    public List<User> Users { get; init; } = [];

    /// <summary>
    /// Adds a user to the study group.
    /// </summary>
    /// <param name="user">The user to add.</param>
    /// <exception cref="InvalidOperationException">Thrown if the user is already in the group.</exception>
    public void AddUser(User user)
    {
        if (Users.Exists(u => u.Id == user.Id))
            throw new InvalidOperationException("User already in group.");

        Users.Add(user);
    }

    /// <summary>
    /// Removes a user from the study group.
    /// </summary>
    /// <param name="user">The user to remove.</param>
    public void RemoveUser(User user)
    {
        Users.RemoveAll(u => u.Id == user.Id);
    }
}