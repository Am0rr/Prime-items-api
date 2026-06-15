using PI.DAL.Enums;

namespace PI.DAL.Entities.Identity;

public class User : BaseEntity
{
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }

    protected User() { }

    public User(string username, string email, string passwordHash, UserRole role = UserRole.Registered)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    public void ChangeUsername(string newUsername) => Username = newUsername;
    public void ChangeEmail(string newEmail) => Email = newEmail;
    public void ChangeRole(UserRole newRole) => Role = newRole;
}