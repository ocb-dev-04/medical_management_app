using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Services.Auth.Application.UseCases;
using Services.Auth.Domain.Errors;
using Shared.Common.Helper.ErrorsHandler;
using System.Reflection.Metadata;

namespace Services.Auth.Application.UnitTests.UseCases.RefreshToken;

public sealed class RefreshTokenQueryHandlerTest
    : BaseTestSharedConfiguration
{
    private readonly RefreshTokenQuery _query;
    private readonly RefreshTokenQueryHandler _handler;

    public RefreshTokenQueryHandlerTest()
    {
        _query = new();
        _handler = new(
            _credentialRepositoryMock.Object,
            _tokenProviderMock.Object,
            _jwtSettingsMock.Object,
            _httpRequestProviderMock.Object,
            _jwtSecurityTokenHandlerMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult()
    {
        // arrange
        Set_Provider_ReadJwt_Success(_validCredential.Id.Value);
        Set_Credential_ByIdAsync_Success();

        // act
        Result<RefreshTokenResponse> result = await _handler.Handle(_query, default);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenCurrentReadJwtFailed()
    {
        // arrange
        Set_Provider_ReadJwt_UnauthorizedFailure();

        // act
        Result<RefreshTokenResponse> result = await _handler.Handle(_query, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenIdWasNotFound()
    {
        // arrange
        Set_Provider_ReadJwt_Success(_validCredential.Id.Value);
        Set_Credential_ByIdAsync_NotFoundFailure();

        // act
        Result<RefreshTokenResponse> result = await _handler.Handle(_query, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CredentialErrors.NotFound);
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
