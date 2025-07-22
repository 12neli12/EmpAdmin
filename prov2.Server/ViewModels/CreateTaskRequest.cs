namespace prov2.Server.ViewModels;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProjectId { get; set; }
    public int AssignedToId { get; set; }
}