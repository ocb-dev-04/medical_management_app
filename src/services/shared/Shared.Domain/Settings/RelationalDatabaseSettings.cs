using System.ComponentModel.DataAnnotations;

namespace Shared.Domain.Settings;

public sealed class RelationalDatabaseSettings : BaseSettings
{
    public int MaxRetryCount { get; set; } = 3;
    public int CommandTimeout { get; set; } = 5;
}
