﻿using Common.Services.Hashing.Abstractions;
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

namespace Services.Auth.Application.UnitTests.UseCases;

public sealed class SigninCommandHandlerTest
{
    private readonly Faker _faker;

    private readonly Mock<ICredentialRepository> _credentialRepositoryMock;
    private readonly Mock<IHashingService> _hashingServiceMock;
    private readonly Mock<ITokenProvider> _tokenProviderMock;

    private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
    private readonly Mock<JwtSecurityTokenHandler> _jwtSecurityTokenHandlerMock;

    private const string ValidPassword = "Qwerty1234@"; // Bogus crashes when trying to create a password using a regular expression pattern

    public SigninCommandHandlerTest()
    {
        _faker = new();

        _credentialRepositoryMock = new();
        _hashingServiceMock = new();
        _tokenProviderMock = new();

        _jwtSettingsMock = new();
        _jwtSecurityTokenHandlerMock = new();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenEmailNotFound()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email, ValidPassword); 
        SigninCommandHandler handle = new SigninCommandHandler(
            _credentialRepositoryMock.Object,
            _hashingServiceMock.Object,
            _tokenProviderMock.Object,
            _jwtSettingsMock.Object,
            _jwtSecurityTokenHandlerMock.Object);

        _credentialRepositoryMock.Setup(
            x => x.ByEmailAsync(
                It.IsAny<EmailAddress>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Credential>(CredentialErrors.EmailNotFound));

        // act
        Result<SigninResponse> result = await handle.Handle(command, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CredentialErrors.EmailNotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenEmailNotValid()
    {
        // arrange
        SigninCommand command = new SigninCommand(_faker.Person.Email.Replace('@', '$'), ValidPassword);
        SigninCommandHandler handle = new SigninCommandHandler(
            _credentialRepositoryMock.Object,
            _hashingServiceMock.Object,
            _tokenProviderMock.Object,
            _jwtSettingsMock.Object,
            _jwtSecurityTokenHandlerMock.Object);

        // act
        Result<SigninResponse> result = await handle.Handle(command, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}
