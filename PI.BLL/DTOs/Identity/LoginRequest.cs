namespace PI.BLL.DTOs.Identity;

public record LoginRequest(
    string Email,
    string Password
);