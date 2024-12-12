using Refit;

namespace Doctor.Management.Gateway.AuthClient;

public interface IAuthClient
{
    [Get("")]
    Task<ApiResponse<AuthResponse>> ValidateAsync([Header("Authorization")] string token, CancellationToken cancellationToken);
}

public sealed record AuthResponse(
    Guid Id,
    string Email,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset ModifiedOnUtc);