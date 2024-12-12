using System.ComponentModel.DataAnnotations;

namespace Doctor.Management.Gateway.Settings;

public sealed class AuthSettings
{
    [Required]
    public string Endpoint { get; set; }
}
