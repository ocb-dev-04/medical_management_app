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
using Value.Objects.Helper.Values.Primitives;
using Services.Auth.Domain.Abstractions.Providers;

namespace Services.Auth.Application.UseCases;

internal sealed class SignUpCommandHandler
    : ICommandHandler<SignupCommand, SignupResponse>
{
    private readonly ICredentialRepository _credentialRepository;
    private readonly IHashingService _hashService;
    private readonly ITokenProvider _tokenProvider;

    private readonly JwtSettings _jwtSettings;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

    public SignUpCommandHandler(
        ICredentialRepository credentialRepository,
        IHashingService hashService,
        ITokenProvider tokenProvider,

        IOptions<JwtSettings> jwtSettings,
        JwtSecurityTokenHandler jwtSecurityTokenHandler)

    {
        ArgumentNullException.ThrowIfNull(credentialRepository, nameof(credentialRepository));
        ArgumentNullException.ThrowIfNull(hashService, nameof(hashService));

        ArgumentNullException.ThrowIfNull(jwtSettings, nameof(jwtSettings));
        ArgumentNullException.ThrowIfNull(tokenProvider, nameof(tokenProvider));
        ArgumentNullException.ThrowIfNull(jwtSecurityTokenHandler, nameof(jwtSecurityTokenHandler));

        _credentialRepository = credentialRepository;
        _hashService = hashService;

        _jwtSettings = jwtSettings.Value;
        _tokenProvider = tokenProvider;
        _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
    }
    public async Task<Result<SignupResponse>> Handle(SignupCommand request, CancellationToken cancellationToken)
    {
        Result<EmailAddress> email = EmailAddress.Create(request.Email);
        if (email.IsFailure)
            return Result.Failure<SignupResponse>(email.Error);

        bool exist = await _credentialRepository.ExistAsync(e => e.Email.Equals(email.Value),  cancellationToken);
        if (exist)
            return Result.Failure<SignupResponse>(CredentialErrors.EmailAlreadyExist);
        
        string hashedValueResult = _hashService.Hash(request.Password);
        Credential credential = Credential.Create(
            email.Value,
            StringObject.Create(hashedValueResult),
            in _hashService);
        
        await _credentialRepository.CreateAsync(credential, cancellationToken);
        await _credentialRepository.CommitAsync(cancellationToken);

        string token = _tokenProvider.BuildJwt(credential, in _jwtSettings, in _jwtSecurityTokenHandler);
        return new SignupResponse(token, CredentialResponse.Map(credential));
    }
}
