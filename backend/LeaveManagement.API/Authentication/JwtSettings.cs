namespace LeaveManagement.API.Authentication;

public class JwtSettings
{
    public string Key { get; set; } = "ChangeThisDevelopmentOnlySecretKeyWithAtLeast32Characters";
    public string Issuer { get; set; } = "LeaveManagement.API";
    public string Audience { get; set; } = "LeaveManagement.Angular";
    public double DurationInMinutes { get; set; } = 120;
}
