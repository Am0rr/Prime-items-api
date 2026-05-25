namespace PI.PL.Infrastructure.Identity;

public class JwtOptions
{
    public string SecureKey { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int AccessTokenLifetimeInMinutes { get; set; }
}