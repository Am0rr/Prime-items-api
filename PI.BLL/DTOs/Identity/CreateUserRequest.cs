namespace PI.BLL.DTOs.Identity;

public record CreateUserRequest(
    string Username,
    string Email,
    string Password
);