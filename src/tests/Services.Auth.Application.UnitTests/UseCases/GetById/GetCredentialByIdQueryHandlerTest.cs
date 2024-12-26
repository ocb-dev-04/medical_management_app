using Moq;
using Bogus;
using FluentAssertions;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.StrongIds;
using Value.Objects.Helper.Values.Domain;
using Services.Auth.Application.UseCases;
using Shared.Common.Helper.ErrorsHandler;
using Common.Services.Hashing.Abstractions;
using Value.Objects.Helper.Values.Primitives;
using Services.Auth.Domain.Abstractions.Repositories;
using Microsoft.AspNetCore.Http;
using Services.Auth.Domain.Errors;

namespace Services.Auth.Application.UnitTests.UseCases.GetById;

public sealed class GetCredentialByIdQueryHandlerTest
{
    private readonly Faker _faker;

    private readonly Mock<ICredentialRepository> _credentialRepositoryMock;
    private readonly Mock<IHashingService> _hashingServiceMock;

    private readonly GetCredentialByIdQuery _validQuery;
    private readonly GetCredentialByIdQueryHandler _handler;
    private readonly Credential _exampleCredential;

    public GetCredentialByIdQueryHandlerTest()
    {
        _faker = new();

        _credentialRepositoryMock = new();
        _hashingServiceMock = new();

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
        _credentialRepositoryMock.Setup(
                x => x.ByIdAsync(
                    It.IsAny<CredentialId>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(_exampleCredential));

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
        _credentialRepositoryMock.Setup(
                x => x.ByIdAsync(
                    It.IsAny<CredentialId>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<Credential>(CredentialErrors.NotFound));

        // act
        Result result = await _handler.Handle(query, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CredentialErrors.NotFound);
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

}
