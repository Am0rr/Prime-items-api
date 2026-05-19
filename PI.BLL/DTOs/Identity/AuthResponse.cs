namespace PI.BLL.DTOs.Identity;

public record AuthResponse
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
    public Guid UserId { get; init; }
    public string Username { get; init; } = null!;
    public string Role { get; init; } = null!;
}