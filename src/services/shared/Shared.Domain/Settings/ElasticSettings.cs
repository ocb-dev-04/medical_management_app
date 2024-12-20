using System.ComponentModel.DataAnnotations;

namespace Shared.Domain.Settings;

public sealed class ElasticSettings
{
    [Required]
    public string Url { get; set; }
    [Required]
    public string DefaultIndex { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Passwword { get; set; }
}
