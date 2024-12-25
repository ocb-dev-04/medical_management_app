using Bogus;
using Common.Services.Hashing.Abstractions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Services.Auth.Application.UseCases;
using Services.Auth.Domain.Abstractions.Providers;
using Services.Auth.Domain.Abstractions.Repositories;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.Errors;
using Services.Auth.Domain.Settings;
using Shared.Common.Helper.ErrorsHandler;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using Value.Objects.Helper.Values.Domain;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Auth.Application.UnitTests.UseCases.Signup;

public sealed class SignUpCommandHandlerTest
{
    private readonly Faker _faker;

    private readonly Mock<ICredentialRepository> _credentialRepositoryMock;
    private readonly Mock<IHashingService> _hashingServiceMock;
    private readonly Mock<ITokenProvider> _tokenProviderMock;

    private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
    private readonly Mock<JwtSecurityTokenHandler> _jwtSecurityTokenHandlerMock;

    private readonly SignUpCommandHandler _handler;
    private const string ValidPassword = "Qwerty1234@"; // Bogus crashes when trying to create a password using a regular expression pattern

    public SignUpCommandHandlerTest()
    {
        _faker = new();

        _credentialRepositoryMock = new();
        _hashingServiceMock = new();
        _tokenProviderMock = new();

        _jwtSettingsMock = new();
        _jwtSecurityTokenHandlerMock = new();

        _handler = new SignUpCommandHandler(
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
        SignupCommand command = new SignupCommand(_faker.Person.Email, ValidPassword);
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
        Result<SignupResponse> result = await _handler.Handle(command, default);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenEmailAlreadyExists()
    {
        // arrange
        SignupCommand command = new SignupCommand(_faker.Person.Email, ValidPassword);
        _credentialRepositoryMock.Setup(
            x => x.ExistAsync(
                It.IsAny<Expression<Func<Credential, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // act
        Result<SignupResponse> result = await _handler.Handle(command, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CredentialErrors.EmailAlreadyExist);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenEmailNotValid()
    {
        // arrange
        SignupCommand command = new SignupCommand(_faker.Person.Email.Replace('@', '$'), ValidPassword);

        // act
        Result<SignupResponse> result = await _handler.Handle(command, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}
