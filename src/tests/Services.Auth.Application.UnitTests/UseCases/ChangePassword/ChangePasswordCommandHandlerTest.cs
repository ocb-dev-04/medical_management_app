using Moq;
using Bogus;
using FluentAssertions;
using Shared.Common.Helper.Models;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.StrongIds;
using Services.Auth.Application.UseCases;
using Shared.Common.Helper.ErrorsHandler;
using Value.Objects.Helper.Values.Domain;
using Common.Services.Hashing.Abstractions;
using Value.Objects.Helper.Values.Primitives;
using Shared.Common.Helper.Abstractions.Providers;
using Services.Auth.Domain.Abstractions.Repositories;
using Microsoft.AspNetCore.Http;
using Services.Auth.Domain.Errors;

namespace Services.Auth.Application.UnitTests.UseCases.ChangePassword;

public sealed class ChangePasswordCommandHandlerTest
{
    private readonly Faker _faker;

    private readonly Mock<ICredentialRepository> _credentialRepositoryMock;
    private readonly Mock<IHashingService> _hashingServiceMock;

    private readonly Mock<IHttpRequestProvider> _httpRequestProviderMock;
    private readonly Mock<IEntitiesEventsManagementProvider> _entitiesEventsManagemmentProvider;

    private readonly ChangePasswordCommand _validCommand;
    private readonly ChangePasswordCommandHandler _handler;
    private readonly CurrentRequestUser _currentRequestUser;
    private readonly Credential _exampleCredential;

    public ChangePasswordCommandHandlerTest()
    {
        _faker = new();

        _credentialRepositoryMock = new();
        _hashingServiceMock = new();

        _httpRequestProviderMock = new();
        _entitiesEventsManagemmentProvider = new();

        string validPassword = "Qwerty1234@"; // Bogus crashes when trying to create a password using a regular expression pattern
        string newValidPassword = "Qwerty1234."; // Bogus crashes when trying to create a password using a regular expression pattern
        _validCommand = new ChangePasswordCommand(validPassword, newValidPassword, newValidPassword);

        _handler = new ChangePasswordCommandHandler(
            _hashingServiceMock.Object,
            _credentialRepositoryMock.Object,
            _httpRequestProviderMock.Object,
            _entitiesEventsManagemmentProvider.Object);

        _currentRequestUser = new CurrentRequestUser(
            Guid.NewGuid(),
            _faker.Person.Email,
            _faker.Commerce.Locale,
            _faker.Random.String(20));

        _exampleCredential = Credential.Create(
            EmailAddress.Create(_faker.Person.Email).Value,
            StringObject.Create(_faker.Internet.Password(20)),
            _hashingServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult()
    {
        // arrange
        Set_Provider_GetCoontextCurrentUser_Success();
        Set_Credential_ByIdAsync_Success();
        Set_Hashing_Verify_AsTrue();

        // act
        Result result = await _handler.Handle(_validCommand, default);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_CurrentUserNotFound_Unauthorized()
    {
        // arrange
        _httpRequestProviderMock.Setup(
            x => x.GetContextCurrentUser())
            .Returns(Result.Failure<CurrentRequestUser>(Error.Unauthorized()));

        // act
        Result result = await _handler.Handle(_validCommand, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Handle_Should_ById_NotFound()
    {
        // arrange
        Set_Provider_GetCoontextCurrentUser_Success();
        _credentialRepositoryMock.Setup(
                x => x.ByIdAsync(
                    It.IsAny<CredentialId>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<Credential>(CredentialErrors.NotFound));

        // act
        Result result = await _handler.Handle(_validCommand, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CredentialErrors.NotFound);
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Handle_Should_Verify_WrongPassword()
    {
        // arrange
        Set_Provider_GetCoontextCurrentUser_Success();
        Set_Credential_ByIdAsync_Success();
        _hashingServiceMock.Setup(
                x => x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(false);

        // act
        Result result = await _handler.Handle(_validCommand, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CredentialErrors.WrongPassword);
        result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    #region Private methods

    private void Set_Provider_GetCoontextCurrentUser_Success()
        => _httpRequestProviderMock.Setup(
            x => x.GetContextCurrentUser())
            .Returns(Result.Success(_currentRequestUser));

    private void Set_Credential_ByIdAsync_Success()
        => _credentialRepositoryMock.Setup(
                x => x.ByIdAsync(
                    It.IsAny<CredentialId>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(_exampleCredential));

    private void Set_Hashing_Verify_AsTrue()
        => _hashingServiceMock.Setup(
                x => x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(true);

    #endregion
}
