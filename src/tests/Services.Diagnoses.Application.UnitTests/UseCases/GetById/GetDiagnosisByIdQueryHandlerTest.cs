using NSubstitute;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Shared.Common.Helper.ErrorsHandler;
using Value.Objects.Helper.Values.Primitives;
using Service.Diagnoses.Application.UseCases;

namespace Services.Diagnoses.Application.UnitTests.UseCases.GetById;

public sealed class GetDiagnosisByIdQueryHandlerTest
    : BaseTestSharedConfiguration
{
    private readonly GetDiagnosisByIdQuery _query;
    private readonly GetDiagnosisByIdQueryHandler _handler;

    public GetDiagnosisByIdQueryHandlerTest()
    {
        _query = new(_validDiagnosis.Id.Value);
        _handler = new(_diagnosisRepositoryMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult()
    {
        // arrange
        Set_GetDiagnosisById_Success();

        // act
        Result result = await _handler.Handle(_query, default);

        // assert
        await _diagnosisRepositoryMock.Received(1)
            .ByIdAsync(Arg.Is<GuidObject>(f
                => f.Value.Equals(_query.Id)), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailedResult_DiagnosisByIdNotFound()
    {
        // arrange
        Set_GetDiagnosisById_NotFound();

        // act
        Result<DiagnosisResponse> result = await _handler.Handle(_query, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}