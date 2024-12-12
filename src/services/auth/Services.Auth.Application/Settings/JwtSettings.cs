using System.ComponentModel.DataAnnotations;

namespace Services.Auth.Application.Settings;

public sealed class JwtSettings
{
    [Required]
    public bool ValidateIssuerSigningKey { get; set; }
    [Required]
    public string IssuerSigningKey { get; set; }

    [Required]
    public bool RequireExpirationTime { get; set; }
    public bool ValidateLifetime { get; set; } = true;

    public bool ValidateIssuer { get; set; } = true;
    [Required]
    public string ValidIssuer { get; set; }

    public bool ValidateAudience { get; set; } = true;
    [Required]
    public string ValidAudience { get; set; }
}
