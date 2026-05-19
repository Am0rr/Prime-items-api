namespace PI.DAL.Entities.Identity;

public class RefreshToken : BaseEntity
{
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    protected RefreshToken() { }

    private RefreshToken(string token, DateTime expiresAt, Guid userId)
    {
        Token = token;
        ExpiresAt = expiresAt;
        IsRevoked = false;
        UserId = userId;
    }

    public static RefreshToken Create(string token, DateTime expiresAt, Guid userId)
    {
        return new RefreshToken(token, expiresAt, userId);
    }

    public void Revoke()
    {
        IsRevoked = true;
    }

}