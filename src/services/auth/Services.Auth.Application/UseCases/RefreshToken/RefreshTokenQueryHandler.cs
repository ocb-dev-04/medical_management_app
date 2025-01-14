using CQRS.MediatR.Helper.Abstractions.Messaging;
using Microsoft.Extensions.Options;
using Services.Auth.Domain.Settings;
using Services.Auth.Domain.Abstractions.Repositories;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.StrongIds;
using Shared.Common.Helper.ErrorsHandler;
using System.IdentityModel.Tokens.Jwt;
using Services.Auth.Domain.Abstractions.Providers;
using Shared.Common.Helper.Abstractions.Providers;

namespace Services.Auth.Application.UseCases;

internal sealed class RefreshTokenQueryHandler
    : IQueryHandler<RefreshTokenQuery, RefreshTokenResponse>
{
    private readonly ICredentialRepository _credentialRepository;
    private readonly ITokenProvider _tokenProvider;

    private readonly JwtSettings _jwtSettings;
    private readonly IHttpRequestProvider _httpRequestProvider;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

    public RefreshTokenQueryHandler(
        ICredentialRepository credentialRepository,
        ITokenProvider tokenProvider,

        IOptions<JwtSettings> jwtSettings,
        IHttpRequestProvider httpRequestProvider,
        JwtSecurityTokenHandler jwtSecurityTokenHandler)

    {
        ArgumentNullException.ThrowIfNull(credentialRepository, nameof(credentialRepository));
        ArgumentNullException.ThrowIfNull(tokenProvider, nameof(tokenProvider));

        ArgumentNullException.ThrowIfNull(jwtSettings, nameof(jwtSettings));
        ArgumentNullException.ThrowIfNull(jwtSecurityTokenHandler, nameof(jwtSecurityTokenHandler));
        ArgumentNullException.ThrowIfNull(httpRequestProvider, nameof(httpRequestProvider));

        _credentialRepository = credentialRepository;
        _tokenProvider = tokenProvider;

        _jwtSettings = jwtSettings.Value;
        _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
        _httpRequestProvider = httpRequestProvider;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
    {
        Result<Guid> credentialIdFromToken = _tokenProvider.ReadJwt(
            in _jwtSettings,
            in _jwtSecurityTokenHandler,
            in _httpRequestProvider);
        if (credentialIdFromToken.IsFailure)
            return Result.Failure<RefreshTokenResponse>(Error.Unauthorized());

        Result<CredentialId> credentialId = CredentialId.Create(credentialIdFromToken.Value);
        if (credentialId.IsFailure)
            return Result.Failure<RefreshTokenResponse>(credentialId.Error);

        Result<Credential> found = await _credentialRepository.ByIdAsync(credentialId.Value, cancellationToken);
        if (found.IsFailure)
            return Result.Failure<RefreshTokenResponse>(found.Error);

        string token = _tokenProvider.BuildJwt(
            found.Value, 
            in _jwtSettings, 
            in _jwtSecurityTokenHandler);

        return new RefreshTokenResponse(token);
    }
}