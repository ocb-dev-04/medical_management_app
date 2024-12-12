using System.ComponentModel.DataAnnotations;

namespace Shared.Consul.Configuration.Settings;

public sealed class ConsulSettings
{
    [Required]
    public string Url { get; set; }
    [Required]
    public string Token { get; set; }
}
