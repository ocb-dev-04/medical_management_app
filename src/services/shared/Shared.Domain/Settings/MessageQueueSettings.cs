using System.ComponentModel.DataAnnotations;

namespace Shared.Domain.Settings;

public sealed class MessageQueueSettings : BaseSettings
{
    [Required]
    public string Url { get; set; }

    [Required]
    public string User { get; set; }

    [Required]
    public string Password { get; set; }
}
