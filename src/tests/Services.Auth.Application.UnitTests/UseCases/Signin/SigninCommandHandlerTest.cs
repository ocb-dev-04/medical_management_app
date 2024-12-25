using Common.Services.Hashing.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Services.Auth.Domain.Settings;
using Services.Auth.Application.UseCases;
using Services.Auth.Domain.Abstractions.Repositories;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.Errors;
using Shared.Common.Helper.ErrorsHandler;
using System.IdentityModel.Tokens.Jwt;
using Value.Objects.Helper.Values.Domain;
using Services.Auth.Domain.Abstractions.Providers;
using Bogus;
using Microsoft.AspNetCore.Http;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Auth.Application.UnitTests.UseCases.Signin;

public sealed class SigninCommandHandlerTest
{
    private readonly Faker _faker;

    private readonly Mock<ICredentialRepository> _credentialRepositoryMock;
    private readonly Mock<IHashingService> _hashingServiceMock;
    private readonly Mock<ITokenProvider> _tokenProviderMock;

    private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
    private readonly Mock<JwtSecurityTokenHandler> _jwtSecurityTokenHandlerMock;

    private readonly SigninCommandHandler _handler;
    private const string ValidPassword = "Qwerty1234@"; // Bogus crashes when trying to create a password using a regular expression pattern

    public SigninCommandHandlerTest()
    {
        _faker = new();

        _credentialRepositoryMock = new();
        _hashingServiceMock = new();
        _tokenProviderMock = new();

        _jwtSettingsMock = new();
        _jwtSecurityTokenHandlerMock = new();

        _handler = new SigninCommandHandler(
            _credentialRepositoryMock.Object,
            _hashingServiceMock.Object,
            _tokenProviderMock.Object,
            _jwtSettingsMock.Object,
            _jwtSecurityTokenHandlerMock.Object);

    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, ValidPassword);
        Credential _exampleCredential = Credential.Create(
            EmailAddress.Create(_faker.Person.Email).Value,
            StringObject.Create(ValidPassword),
            _hashingServiceMock.Object);

        _credentialRepositoryMock.Setup(
            x => x.ByEmailAsync(
                It.IsAny<EmailAddress>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<Credential>(_exampleCredential));

        _hashingServiceMock.Setup(
            x => x.Hash(It.IsAny<string>()))
            .Returns(ValidPassword);

        // act
        Result<SigninResponse> result = await _handler.Handle(command, default);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenEmailNotFound()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, ValidPassword); 
        _credentialRepositoryMock.Setup(
            x => x.ByEmailAsync(
                It.IsAny<EmailAddress>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Credential>(CredentialErrors.EmailNotFound));

        // act
        Result<SigninResponse> result = await _handler.Handle(command, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CredentialErrors.EmailNotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenEmailNotValid()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email.Replace('@', '$'), ValidPassword);

        // act
        Result<SigninResponse> result = await _handler.Handle(command, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WrongPassword()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, ValidPassword);
        Credential _exampleCredential = Credential.Create(
            EmailAddress.Create(_faker.Person.Email).Value,
            StringObject.Create(_faker.Internet.Password(20)),
            _hashingServiceMock.Object);

        _credentialRepositoryMock.Setup(
            x => x.ByEmailAsync(
                It.IsAny<EmailAddress>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<Credential>(_exampleCredential));

        _hashingServiceMock.Setup(
            x => x.Hash(It.IsAny<string>()))
            .Returns(_faker.Internet.Password(20));

        // act
        Result<SigninResponse> result = await _handler.Handle(command, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        result.Error.Should().Be(CredentialErrors.WrongPassword);
    }
}
