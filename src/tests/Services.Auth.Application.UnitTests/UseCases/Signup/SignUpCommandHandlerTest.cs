using Moq;
using FluentAssertions;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Services.Auth.Domain.Errors;
using Services.Auth.Domain.Entities;
using Services.Auth.Application.UseCases;
using Shared.Common.Helper.ErrorsHandler;
using Value.Objects.Helper.Values.Domain;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Auth.Application.UnitTests.UseCases.Signup;

public sealed class SignUpCommandHandlerTest
    : BaseTestSharedConfiguration
{
    private readonly SignUpCommandHandler _handler;

    public SignUpCommandHandlerTest()
    {
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

        Set_Credential_ExistAsync_AsFalse();

        _hashingServiceMock.Setup(
            x => x.Hash(command.Password))
            .Returns(It.IsAny<string>());

        _credentialRepositoryMock.Setup(
            x => x.CreateAsync(
                _validCredential,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _credentialRepositoryMock.Setup(
            x => x.CommitAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // act
        Result<SignupResponse> result = await _handler.Handle(command, default);

        // assert
        _credentialRepositoryMock.Verify(f
            => f.ExistAsync(
                It.IsAny<Expression<Func<Credential, bool>>>(),
                It.IsAny<CancellationToken>()),
                Times.Once);

        _hashingServiceMock.Verify(f
            => f.Hash(
                It.IsAny<string>()),
                Times.AtLeastOnce);

        _credentialRepositoryMock.Verify(f
            => f.CreateAsync(
                It.IsAny<Credential>(),
                It.IsAny<CancellationToken>()),
                Times.Once);

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
