using prov2.Server.Entities;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool IsDeleted { get; set; } = false;

    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
}
