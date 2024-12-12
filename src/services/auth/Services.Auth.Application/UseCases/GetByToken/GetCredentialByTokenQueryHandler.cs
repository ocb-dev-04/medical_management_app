using Shared.Common.Helper.ErrorsHandler;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using Shared.Common.Helper.Models;
using Shared.Common.Helper.Providers;
using Services.Auth.Domain.Abstractions;
using Services.Auth.Domain.StrongIds;
using Services.Auth.Domain.Entities;
using Microsoft.Extensions.Options;
using Services.Auth.Application.Providers;
using Services.Auth.Application.Settings;
using System.IdentityModel.Tokens.Jwt;

namespace Services.Auth.Application.UseCases;

internal sealed class GetCredentialByTokenQueryHandler
    : IQueryHandler<GetCredentialByTokenQuery, CredentialResponse>
{

    private readonly ICredentialRepository _credentialRepository;

    private readonly JwtSettings _jwtSettings;
    private readonly TokenProvider _tokenProvider;
    private readonly HttpRequestProvider _httpRequestProvider;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

    public GetCredentialByTokenQueryHandler(
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

    public async Task<Result<CredentialResponse>> Handle(GetCredentialByTokenQuery request, CancellationToken cancellationToken)
    {
        Result<Guid> credentialIdFromToken = _tokenProvider.ReadJwt(
            in _jwtSettings,
            in _jwtSecurityTokenHandler,
            in _httpRequestProvider);
        if (credentialIdFromToken.IsFailure)
            return Result.Failure<CredentialResponse>(Error.Unauthorized());

        Result<CredentialId> credendialId = CredentialId.Create(credentialIdFromToken.Value);
        if(credendialId.IsFailure)
            return Result.Failure<CredentialResponse>(credendialId.Error);

        Result<Credential> found = await _credentialRepository.ByIdAsync(credendialId.Value, cancellationToken);
        if (found.IsFailure)
            return Result.Failure<CredentialResponse>(found.Error);

        return CredentialResponse.Map(found.Value);
    }
}
