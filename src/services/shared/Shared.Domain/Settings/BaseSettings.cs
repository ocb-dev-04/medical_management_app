using System.ComponentModel.DataAnnotations;

namespace Shared.Domain.Settings;

public class BaseSettings
{
    [Required]
    public string ConnectionString { get; set; }
}
