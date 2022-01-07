namespace Sample.Entities;
public class UserToken
{
    public long Id { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow;

    public string Purpose { get; set; } = string.Empty;
}
