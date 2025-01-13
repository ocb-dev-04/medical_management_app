using NSubstitute;
using FluentAssertions;
using Shared.Common.Helper.ErrorsHandler;
using Service.Diagnoses.Application.UseCases;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Diagnoses.Application.UnitTests.UseCases.GetCollectionByPatientId;

public sealed class GetDiagnosisCollectionByPatientIdQueryHandlerTest
    : BaseTestSharedConfiguration
{
    private readonly GetDiagnosisCollectionByPatientIdQuery _query;
    private readonly GetDiagnosisCollectionByPatientIdQueryHandler _handler;

    public GetDiagnosisCollectionByPatientIdQueryHandlerTest()
    {
        _query = new(_validPatient.Id, 1);
        _handler = new(_diagnosisRepositoryMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult()
    {
        // arrange
        Set_GetDiagnosisCollectionByPatientById_Success();

        // act
        Result<IEnumerable<DiagnosisResponse>> result = await _handler.Handle(_query, default);

        // assert
        await _diagnosisRepositoryMock.Received(1)
            .ByPatientId(Arg.Is<GuidObject>(f
                => f.Value.Equals(_query.Id)), 1, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Any().Should().BeTrue();
    }
    
    [Fact]
    public async Task Handle_Should_ReturnSuccessResult_ButEmpty()
    {
        // arrange
        Set_GetDiagnosisCollectionByPatientById_Empty();

        // act
        Result<IEnumerable<DiagnosisResponse>> result = await _handler.Handle(_query, default);

        // assert
        await _diagnosisRepositoryMock.Received(1)
            .ByPatientId(Arg.Is<GuidObject>(f
                => f.Value.Equals(_query.Id)), 1, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Any().Should().BeFalse();
    }
}
