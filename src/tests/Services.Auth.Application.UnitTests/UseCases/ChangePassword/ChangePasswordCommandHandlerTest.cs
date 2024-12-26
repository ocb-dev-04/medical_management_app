using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Services.Auth.Domain.Errors;
using Services.Auth.Application.UseCases;
using Shared.Common.Helper.ErrorsHandler;

namespace Services.Auth.Application.UnitTests.UseCases.ChangePassword;

public sealed class ChangePasswordCommandHandlerTest
    : BaseTestSharedConfiguration
{
    private readonly ChangePasswordCommand _validCommand;
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTest()
    {
        string validPassword = "Qwerty1234@"; // Bogus crashes when trying to create a password using a regular expression pattern
        string newValidPassword = "Qwerty1234."; // Bogus crashes when trying to create a password using a regular expression pattern
        _validCommand = new ChangePasswordCommand(validPassword, newValidPassword, newValidPassword);

        _handler = new ChangePasswordCommandHandler(
            _hashingServiceMock.Object,
            _credentialRepositoryMock.Object,
            _httpRequestProviderMock.Object,
            _entitiesEventsManagemmentProvider.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult()
    {
        // arrange
        Set_Provider_GetContextCurrentUser_Success();
        Set_Credential_ByIdAsync_Success();
        Set_Hashing_Verify_AsTrue();

        // act
        Result result = await _handler.Handle(_validCommand, default);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenCurrentUserNotFound()
    {
        // arrange
        Set_Provider_GetContextCurrentUser_UnauthorizedFailure();

        // act
        Result result = await _handler.Handle(_validCommand, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenIdWasNotFound()
    {
        // arrange
        Set_Provider_GetContextCurrentUser_Success();
        Set_Credential_ByIdAsync_NotFoundFailure();

        // act
        Result result = await _handler.Handle(_validCommand, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CredentialErrors.NotFound);
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenPasswordIsWrong()
    {
        // arrange
        Set_Provider_GetContextCurrentUser_Success();
        Set_Credential_ByIdAsync_Success();
        Set_Hashing_Verify_AsFalse();

        // act
        Result result = await _handler.Handle(_validCommand, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CredentialErrors.WrongPassword);
        result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}
