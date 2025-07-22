namespace prov2.Server.ViewModels
{
    public class CreateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<int> EmployeeIds { get; set; } = new();
    }
}
