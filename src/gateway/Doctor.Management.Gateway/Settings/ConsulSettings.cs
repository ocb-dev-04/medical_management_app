using System.ComponentModel.DataAnnotations;

namespace Doctor.Management.Gateway.Settings;

public sealed class ConsulSettings
{
    [Required]
    public string Url { get; set; }
    [Required]
    public string Token { get; set; }
}
