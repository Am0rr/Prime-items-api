namespace PI.BLL.Interfaces;

public interface IJwtProvider
{
    string GenerateAccessToken(Guid userId, string username, string userRole);
    string GenerateRefreshToken();
}