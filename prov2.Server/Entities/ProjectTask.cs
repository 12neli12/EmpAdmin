namespace prov2.Server.Entities;

public class ProjectTask
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; } = false;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = default!;

    public int AssignedToId { get; set; }
    public User AssignedTo { get; set; } = default!;

    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = default!;
}
