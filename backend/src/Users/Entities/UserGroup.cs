namespace Users.Entities;

public class UserGroup
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Group Group { get; set; } = null!;
}
