using CQRS.MediatR.Helper.Abstractions.Messaging;
using Microsoft.Extensions.Options;
using Services.Auth.Application.Providers;
using Services.Auth.Application.Settings;
using Services.Auth.Domain.Abstractions;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.StrongIds;
using Shared.Common.Helper.ErrorsHandler;
using Shared.Common.Helper.Providers;
using System.IdentityModel.Tokens.Jwt;

namespace Services.Auth.Application.UseCases;

internal sealed class RefreshTokenQueryHandler
    : IQueryHandler<RefreshTokenQuery, RefreshTokenResponse>
{
    private readonly ICredentialRepository _credentialRepository;

    private readonly JwtSettings _jwtSettings;
    private readonly TokenProvider _tokenProvider;
    private readonly HttpRequestProvider _httpRequestProvider;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

    public RefreshTokenQueryHandler(
        ICredentialRepository credentialRepository,
        IOptions<JwtSettings> jwtSettings,
        TokenProvider tokenProvider,
        HttpRequestProvider httpRequestProvider,
        JwtSecurityTokenHandler jwtSecurityTokenHandler)

    {
        ArgumentNullException.ThrowIfNull(credentialRepository, nameof(credentialRepository));
        ArgumentNullException.ThrowIfNull(jwtSettings, nameof(jwtSettings));
        ArgumentNullException.ThrowIfNull(tokenProvider, nameof(tokenProvider));
        ArgumentNullException.ThrowIfNull(jwtSecurityTokenHandler, nameof(jwtSecurityTokenHandler));
        ArgumentNullException.ThrowIfNull(httpRequestProvider, nameof(httpRequestProvider));

        _credentialRepository = credentialRepository;
        _jwtSettings = jwtSettings.Value;
        _tokenProvider = tokenProvider;
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

        Result<string> token = _tokenProvider.BuildJwt(
            found.Value, 
            in _jwtSettings, 
            in _jwtSecurityTokenHandler);
        if (token.IsFailure)
            return Result.Failure<RefreshTokenResponse>(token.Error);

        return new RefreshTokenResponse(token.Value);
    }
}