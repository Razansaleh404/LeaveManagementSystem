namespace LeaveManagement.API.Authentication;

public class JwtSettings
{
    public string Issuer { get; set; } = "LeaveManagement.API";
    public string Audience { get; set; } = "LeaveManagement.Angular";
    public string Secret { get; set; } = "ChangeThisDevelopmentOnlySecretKeyWithAtLeast32Characters";
    public int ExpirationMinutes { get; set; } = 120;
}
