using Consul;
using System.ComponentModel.DataAnnotations;

namespace Shared.Consul.Configuration.Settings;

public sealed class ServiceRegistrationSettings
{
    [Required]
    public string Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Address { get; set; }
    [Required]
    public int Port { get; set; }
    [Required]
    public ServiceCheck ServiceCheck { get; set; }

    public AgentServiceRegistration MapToAgentRegistration()
        => new()
        {
            ID = this.Id,
            Name = this.Name,
            Address = this.Address,
            Port = this.Port,
            Check = new AgentServiceCheck
            {
                HTTP = this.ServiceCheck.HealthEndpoint,
                Interval = TimeSpan.FromSeconds(this.ServiceCheck.IntervalToCheckInSeconds),
                Timeout = TimeSpan.FromSeconds(this.ServiceCheck.TimeoutCheckInSeconds)
            }
        };
}

public sealed class ServiceCheck
{
    [Required]
    public string HealthEndpoint { get; set; }
    public int IntervalToCheckInSeconds { get; set; } = 10;
    public int TimeoutCheckInSeconds { get; set; } = 5;
}
