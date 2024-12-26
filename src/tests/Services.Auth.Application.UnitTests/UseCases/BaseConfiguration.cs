using Bogus;
using Common.Services.Hashing.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Services.Auth.Domain.Abstractions.Providers;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.Settings;
using System.IdentityModel.Tokens.Jwt;
using Value.Objects.Helper.Values.Domain;
using Value.Objects.Helper.Values.Primitives;
using Services.Auth.Domain.Abstractions.Repositories;
using Services.Auth.Domain.StrongIds;
using Shared.Common.Helper.Models;
using Shared.Common.Helper.Abstractions.Providers;
using Shared.Common.Helper.ErrorsHandler;

namespace Services.Auth.Application.UnitTests.UseCases;

public abstract class BaseConfiguration
{
    protected readonly Faker _faker;

    protected readonly Mock<ICredentialRepository> _credentialRepositoryMock;
    protected readonly Mock<IHashingService> _hashingServiceMock;
    protected readonly Mock<ITokenProvider> _tokenProviderMock;

    protected readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
    protected readonly Mock<JwtSecurityTokenHandler> _jwtSecurityTokenHandlerMock;

    protected readonly Mock<IHttpRequestProvider> _httpRequestProviderMock;
    protected readonly Mock<IEntitiesEventsManagementProvider> _entitiesEventsManagemmentProvider;

    protected readonly CurrentRequestUser _currentRequestUser;
    protected readonly Credential _validCredential;

    protected const string ValidPassword = "Qwerty1234@"; // Bogus crashes when trying to create a password using a regular expression pattern

    protected BaseConfiguration()
    {
        _faker = new();

        _credentialRepositoryMock = new();
        _hashingServiceMock = new();
        _tokenProviderMock = new();

        _jwtSettingsMock = new();
        _jwtSecurityTokenHandlerMock = new();

        _httpRequestProviderMock = new();
        _entitiesEventsManagemmentProvider = new();

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

    protected void Set_Provider_GetCoontextCurrentUser_Success()
        => _httpRequestProviderMock.Setup(
            x => x.GetContextCurrentUser())
            .Returns(Result.Success(_currentRequestUser));

    protected void Set_Credential_ByIdAsync_Success()
        => _credentialRepositoryMock.Setup(
                x => x.ByIdAsync(
                    It.IsAny<CredentialId>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(_validCredential));

    protected void Set_Hashing_Verify_AsTrue()
        => _hashingServiceMock.Setup(
                x => x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(true);

}
