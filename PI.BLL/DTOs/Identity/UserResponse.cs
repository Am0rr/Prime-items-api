namespace PI.BLL.DTOs.Identity;

public record UserResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Role { get; init; } = null!;
}