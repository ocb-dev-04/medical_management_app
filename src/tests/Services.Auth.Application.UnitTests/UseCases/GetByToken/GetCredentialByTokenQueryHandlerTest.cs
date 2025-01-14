using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Services.Auth.Domain.Errors;
using Services.Auth.Application.UseCases;
using Shared.Common.Helper.ErrorsHandler;

namespace Services.Auth.Application.UnitTests.UseCases.GetByToken;

public sealed class GetCredentialByTokenQueryHandlerTest
    : BaseTestSharedConfiguration
{
    private readonly GetCredentialByTokenQuery _query;
    private readonly GetCredentialByTokenQueryHandler _handler;

    public GetCredentialByTokenQueryHandlerTest()
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
        Result<CredentialResponse> result = await _handler.Handle(_query, default);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenCurrentReadJwtFailed()
    {
        // arrange
        Set_Provider_ReadJwt_UnauthorizedFailure();

        // act
        Result<CredentialResponse> result = await _handler.Handle(_query, default);

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
        Result<CredentialResponse> result = await _handler.Handle(_query, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CredentialErrors.NotFound);
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
