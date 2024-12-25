using Common.Services.Hashing.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Services.Auth.Application.Providers;
using Services.Auth.Application.Settings;
using Services.Auth.Application.UseCases;
using Services.Auth.Domain.Abstractions;
using Services.Auth.Domain.Errors;
using Shared.Common.Helper.ErrorsHandler;
using Shared.Common.Helper.Providers;
using System.IdentityModel.Tokens.Jwt;

namespace Services.Auth.Application.UnitTests.UseCases;

public sealed class SigninCommandHandlerTest
{
    private readonly Mock<ICredentialRepository> _credentialRepositoryMock;
    private readonly Mock<IHashingService> _hashingServiceMock;
    private readonly Mock<TokenProvider> _tokenProviderMock;
    private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
    private readonly Mock<JwtSecurityTokenHandler> _jwtSecurityTokenHandlerMock;
    private readonly Mock<EntitiesEventsManagementProvider> _entitiesEventsManagementProviderMock;

    public SigninCommandHandlerTest()
    {
        _credentialRepositoryMock = new();
        _hashingServiceMock = new();
        _tokenProviderMock = new();
        _jwtSettingsMock = new();
        _jwtSecurityTokenHandlerMock = new();
        _entitiesEventsManagementProviderMock = new();
    }

    [Fact]
    public async void Handle_Should_ReturnFailureResult_WhenEmailIsNotUnique()
    {
        // arrange
        SigninCommand command = new SigninCommand("test@test.com", "Qwerrty1234@");
        SigninCommandHandler handle = new SigninCommandHandler(
            _credentialRepositoryMock.Object,
            _hashingServiceMock.Object,
            _tokenProviderMock.Object,
            _jwtSettingsMock.Object,
            _jwtSecurityTokenHandlerMock.Object,
            _entitiesEventsManagementProviderMock.Object);

        // act
        Result<SigninResponse> result = await handle.Handle(command, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CredentialErrors.EmailAlreadyExist);
    }
}
