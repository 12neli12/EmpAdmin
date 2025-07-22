using prov2.Server.Entities;
using prov2.Server.Data;

namespace prov2.Server;

public static class DbSeeder
{
    public static void Seed(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (db.Users.Any()) return; // Skip if already seeded

        var admin = new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Administrator",
            FullName = "Admin User"
        };

        var employee = new User
        {
            Username = "employee",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("emp123"),
            Role = "Employee",
            FullName = "John Employee"
        };

        db.Users.AddRange(admin, employee);
        db.SaveChanges();

        var project = new Project
        {
            Name = "Initial Project",
            Description = "Demo project"
        };
        db.Projects.Add(project);
        db.SaveChanges();

        db.UserProjects.AddRange(
            new UserProject { UserId = admin.Id, ProjectId = project.Id },
            new UserProject { UserId = employee.Id, ProjectId = project.Id }
        );

        db.Tasks.Add(new ProjectTask
        {
            Title = "Initial Task",
            Description = "Demo task",
            ProjectId = project.Id,
            AssignedToId = employee.Id,
            CreatedById = admin.Id
        });

        db.SaveChanges();
    }
}
