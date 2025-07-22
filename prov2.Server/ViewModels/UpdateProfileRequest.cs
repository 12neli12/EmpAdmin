namespace prov2.Server.ViewModels;

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? NewPassword { get; set; }
    public IFormFile? ProfilePicture { get; set; }
}
