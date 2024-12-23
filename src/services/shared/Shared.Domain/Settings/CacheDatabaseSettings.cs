using System.ComponentModel.DataAnnotations;

namespace Shared.Domain.Settings;

public sealed class CacheDatabaseSettings : BaseSettings
{
    [Required]
    public string Password { get; set; }

    public int MaxRetryCount { get; set; } = 3;
    public int Timeout { get; set; } = 10;
}
