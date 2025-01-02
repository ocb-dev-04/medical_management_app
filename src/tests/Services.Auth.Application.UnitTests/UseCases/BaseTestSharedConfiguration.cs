using Moq;
using Bogus;
using Shared.Common.Helper.Models;
using Services.Auth.Domain.Errors;
using Microsoft.Extensions.Options;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.Settings;
using Services.Auth.Domain.StrongIds;
using System.IdentityModel.Tokens.Jwt;
using Value.Objects.Helper.Values.Domain;
using Shared.Common.Helper.ErrorsHandler;
using Common.Services.Hashing.Abstractions;
using Value.Objects.Helper.Values.Primitives;
using Services.Auth.Domain.Abstractions.Providers;
using Shared.Common.Helper.Abstractions.Providers;
using Services.Auth.Domain.Abstractions.Repositories;

namespace Services.Auth.Application.UnitTests.UseCases;

public abstract class BaseTestSharedConfiguration
{
    protected readonly Faker _faker;

    protected readonly Mock<ITokenProvider> _tokenProviderMock;
    protected readonly Mock<IHashingService> _hashingServiceMock;
    protected readonly Mock<IHttpRequestProvider> _httpRequestProviderMock;
    protected readonly Mock<ICredentialRepository> _credentialRepositoryMock;
    protected readonly Mock<IEntitiesEventsManagementProvider> _entitiesEventsManagemmentProvider;

    protected readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
    protected readonly Mock<JwtSecurityTokenHandler> _jwtSecurityTokenHandlerMock;

    protected readonly Credential _validCredential;
    protected readonly CurrentRequestUser _currentRequestUser;

    protected const string ValidPassword = "Qwerty1234@"; // Bogus crashes when trying to create a password using a regular expression pattern

    protected BaseTestSharedConfiguration()
    {
        _faker = new();

        _tokenProviderMock = new();
        _hashingServiceMock = new();
        _httpRequestProviderMock = new();
        _credentialRepositoryMock = new();
        _entitiesEventsManagemmentProvider = new();

        _jwtSettingsMock = new();
        _jwtSecurityTokenHandlerMock = new();

        _validCredential = Credential.Create(
            EmailAddress.Create(_faker.Person.Email).Value,
            StringObject.Create(ValidPassword),
            _hashingServiceMock.Object);

        _currentRequestUser = new CurrentRequestUser(
            _validCredential.Id.Value,
            _faker.Person.Email,
            _faker.Commerce.Locale,
            _faker.Random.String(20));
    }

    #region Token Provider

    // read jwt
    protected void Set_Provider_ReadJwt_Success(Guid id)
        => _tokenProviderMock.Setup(
            x => x.ReadJwt(
                _jwtSettingsMock.Object.Value,
                _jwtSecurityTokenHandlerMock.Object,
                _httpRequestProviderMock.Object))
            .Returns(Result.Success(id));
    
    protected void Set_Provider_ReadJwt_UnauthorizedFailure()
        => _tokenProviderMock.Setup(
                x => x.ReadJwt(
                    _jwtSettingsMock.Object.Value,
                    _jwtSecurityTokenHandlerMock.Object,
                    _httpRequestProviderMock.Object))
                .Returns(Result.Failure<Guid>(Error.Unauthorized()));

    // build jwt
    protected void Set_Provider_BuildJwt_Success()
        => _tokenProviderMock.Setup(
            x => x.BuildJwt(
                _validCredential,
                _jwtSettingsMock.Object.Value,
                _jwtSecurityTokenHandlerMock.Object))
            .Returns(Guid.NewGuid().ToString());

    #endregion

    #region Http Provider

    protected void Set_Provider_GetContextCurrentUser_Success()
        => _httpRequestProviderMock.Setup(
                x => x.GetContextCurrentUser())
                .Returns(Result.Success(_currentRequestUser));

    protected void Set_Provider_GetContextCurrentUser_UnauthorizedFailure()
        => _httpRequestProviderMock.Setup(
                x => x.GetContextCurrentUser())
                .Returns(Result.Failure<CurrentRequestUser>(Error.Unauthorized()));

    #endregion

    #region Credential Repository

    protected void Set_Credential_ByIdAsync_Success()
        => _credentialRepositoryMock.Setup(
                x => x.ByIdAsync(
                    It.IsAny<CredentialId>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(_validCredential));

    protected void Set_Credential_ByIdAsync_NotFoundFailure()
        => _credentialRepositoryMock.Setup(
                x => x.ByIdAsync(
                    It.IsAny<CredentialId>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<Credential>(CredentialErrors.NotFound));

    protected void Set_Credential_ByEmailAsync_Success()
        => _credentialRepositoryMock.Setup(
                x => x.ByEmailAsync(
                    It.IsAny<EmailAddress>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success<Credential>(_validCredential));

    protected void Set_Credential_ByEmailAsync_NotFoundFailure()
        => _credentialRepositoryMock.Setup(
                x => x.ByEmailAsync(
                    It.IsAny<EmailAddress>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<Credential>(CredentialErrors.EmailNotFound));

    #endregion

    #region Hashing service

    protected void Set_Hashing_Verify_AsTrue()
        => _hashingServiceMock.Setup(
                x => x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(true);

    protected void Set_Hashing_Verify_AsFalse()
        => _hashingServiceMock.Setup(
                x => x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(false);

    #endregion
}
