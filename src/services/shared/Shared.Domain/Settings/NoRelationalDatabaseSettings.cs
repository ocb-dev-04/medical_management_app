using System.ComponentModel.DataAnnotations;

namespace Shared.Domain.Settings;

public sealed class NoRelationalDatabaseSettings : BaseSettings
{
    public string DatabaseName { get; set; } = "general_db";
    public IEnumerable<string> Collections { get; set; } = Enumerable.Empty<string>();
}
