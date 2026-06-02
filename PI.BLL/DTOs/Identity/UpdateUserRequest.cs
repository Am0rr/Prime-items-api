namespace PI.BLL.DTOs.Identity;

public record UpdateUserRequest(
    string? Username,
    string? Email,
    string? Role
);