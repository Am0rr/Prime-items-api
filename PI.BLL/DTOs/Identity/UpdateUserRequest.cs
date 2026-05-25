namespace PI.BLL.DTOs.Identity;

public record UpdateUserRequest(
    Guid Id,
    string? Username,
    string? Email,
    string? Role
);