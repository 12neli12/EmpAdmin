namespace prov2.Server.Entities;

public class UserProject
{
    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = default!;
}