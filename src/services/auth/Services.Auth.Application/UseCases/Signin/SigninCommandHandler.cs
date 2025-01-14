using Common.Services.Hashing.Abstractions;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using Microsoft.Extensions.Options;
using Services.Auth.Domain.Settings;
using Services.Auth.Domain.Abstractions.Repositories;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.Errors;
using Shared.Common.Helper.ErrorsHandler;
using System.IdentityModel.Tokens.Jwt;
using Value.Objects.Helper.Values.Domain;
using Services.Auth.Domain.Abstractions.Providers;

namespace Services.Auth.Application.UseCases;

internal sealed class SigninCommandHandler
    : ICommandHandler<SigninCommand, SigninResponse>
{
    private readonly ICredentialRepository _credentialRepository;
    private readonly IHashingService _hashService;
    private readonly ITokenProvider _tokenProvider;

    private readonly JwtSettings _jwtSettings;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

    public SigninCommandHandler(
        ICredentialRepository credentialRepository,
        IHashingService hashService,

        ITokenProvider tokenProvider,
        IOptions<JwtSettings> jwtSettings,
        JwtSecurityTokenHandler jwtSecurityTokenHandler)
    {
        ArgumentNullException.ThrowIfNull(credentialRepository, nameof(credentialRepository));
        ArgumentNullException.ThrowIfNull(hashService, nameof(hashService));

        ArgumentNullException.ThrowIfNull(tokenProvider, nameof(tokenProvider));
        ArgumentNullException.ThrowIfNull(jwtSettings, nameof(jwtSettings));
        ArgumentNullException.ThrowIfNull(jwtSecurityTokenHandler, nameof(jwtSecurityTokenHandler));

        _credentialRepository = credentialRepository;
        _hashService = hashService;

        _tokenProvider = tokenProvider;
        _jwtSettings = jwtSettings.Value;
        _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
    }

    public async Task<Result<SigninResponse>> Handle(
        SigninCommand request,
        CancellationToken cancellationToken)
    {
        Result<EmailAddress> requestEmailResult = EmailAddress.Create(request.Email);
        if (requestEmailResult.IsFailure)
            return Result.Failure<SigninResponse>(requestEmailResult.Error);

        Result<Credential> found = await _credentialRepository.ByEmailAsync(requestEmailResult.Value, cancellationToken: cancellationToken);
        if (found.IsFailure)
            return Result.Failure<SigninResponse>(found.Error);

        string hashedValueResult = _hashService.Hash(request.Password);
        if(!found.Value.Password.Value.Equals(hashedValueResult))
            return Result.Failure<SigninResponse>(CredentialErrors.WrongPassword);

        string token = _tokenProvider.BuildJwt(
            found.Value, 
            in _jwtSettings, 
            in _jwtSecurityTokenHandler);

        return new SigninResponse(token, CredentialResponse.Map(found.Value));
    }
}