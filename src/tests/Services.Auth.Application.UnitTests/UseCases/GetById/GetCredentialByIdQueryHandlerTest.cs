using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Services.Auth.Domain.Errors;
using Services.Auth.Domain.Entities;
using Value.Objects.Helper.Values.Domain;
using Services.Auth.Application.UseCases;
using Shared.Common.Helper.ErrorsHandler;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Auth.Application.UnitTests.UseCases.GetById;

public sealed class GetCredentialByIdQueryHandlerTest
    : BaseTestSharedConfiguration
{
    private readonly GetCredentialByIdQuery _validQuery;
    private readonly GetCredentialByIdQueryHandler _handler;
    private readonly Credential _exampleCredential;

    public GetCredentialByIdQueryHandlerTest()
    {
        _exampleCredential = Credential.Create(
            EmailAddress.Create(_faker.Person.Email).Value,
            StringObject.Create(_faker.Internet.Password(20)),
            _hashingServiceMock.Object);

        _validQuery = new GetCredentialByIdQuery(_exampleCredential.Id.Value);
        _handler = new GetCredentialByIdQueryHandler(_credentialRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult()
    {
        // arrange
        Set_Credential_ByIdAsync_Success();

        // act
        Result<CredentialResponse> result = await _handler.Handle(_validQuery, default);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenIdWasNotFound()
    {
        // arrange
        GetCredentialByIdQuery query = new GetCredentialByIdQuery(Guid.NewGuid());
        Set_Credential_ByIdAsync_NotFoundFailure();

        // act
        Result result = await _handler.Handle(query, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CredentialErrors.NotFound);
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
