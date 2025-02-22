﻿using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Services.Auth.Domain.Errors;
using Services.Auth.Application.UseCases;
using Shared.Common.Helper.ErrorsHandler;
using Value.Objects.Helper.Values.Domain;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.Settings;
using System.IdentityModel.Tokens.Jwt;

namespace Services.Auth.Application.UnitTests.UseCases.Signin;

public sealed class SigninCommandHandlerTest
    : BaseTestSharedConfiguration
{
    private readonly SigninCommandHandler _handler;
    
    public SigninCommandHandlerTest()
    {
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

        Set_Credential_ByEmailAsync_Success();

        _hashingServiceMock.Setup(
            x => x.Hash(ValidPassword))
            .Returns(ValidPassword);

        Set_Provider_BuildJwt_Success();

        // act
        Result<SigninResponse> result = await _handler.Handle(command, default);

        // assert
        _credentialRepositoryMock.Verify(repo
            => repo.ByEmailAsync(
                It.Is<EmailAddress>(e => e.Value.Equals(command.Email)),
                true,
                It.IsAny<CancellationToken>()),
                Times.Once);

        _hashingServiceMock.Verify(service
            => service.Hash(
                It.Is<string>(s => s.Equals(command.Password))),
                Times.Once);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenEmailNotFound()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, ValidPassword);
        Set_Credential_ByEmailAsync_NotFoundFailure();

        // act
        Result<SigninResponse> result = await _handler.Handle(command, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CredentialErrors.EmailNotFound);
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
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
    public async Task Handle_Should_ReturnFailureResult_WhenPasswordIsWrong()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, ValidPassword);
        Set_Credential_ByEmailAsync_Success();

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
