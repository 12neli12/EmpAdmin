namespace prov2.Server.ViewModels;

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? NewPassword { get; set; }
}
