namespace prov2.Server.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee";
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }

    public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
    public ICollection<ProjectTask> AssignedTasks { get; set; } = new List<ProjectTask>();
    public ICollection<ProjectTask> CreatedTasks { get; set; } = new List<ProjectTask>();
}