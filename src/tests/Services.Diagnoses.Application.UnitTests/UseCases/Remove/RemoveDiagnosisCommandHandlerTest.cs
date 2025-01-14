using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Service.Diagnoses.Application.UseCases;
using Shared.Common.Helper.ErrorsHandler;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Diagnoses.Application.UnitTests.UseCases.Remove;

public sealed class RemoveDiagnosisCommandHandlerTest
    : BaseTestSharedConfiguration
{
    private readonly RemoveDiagnosisCommand _command;
    private readonly RemoveDiagnosisCommandHandler _handler;

    public RemoveDiagnosisCommandHandlerTest()
    {
        _command = new(_validDiagnosis.Id.Value, _validDoctor.Id);
        _handler = new(
            _diagnosisRepositoryMock,
            _messageQeueServicesMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult()
    {
        // arrange
        Set_DoctorMessageQueue_Succes();
        Set_GetDiagnosisById_Success();

        // act
        Result result = await _handler.Handle(_command, default);

        // assert
        await _messageQeueServicesMock.Received(1)
            .GetDoctorByIdAsync(Arg.Is<Guid>(f
                => f.Equals(_command.DoctorId)), default);

        await _diagnosisRepositoryMock.Received(1)
            .ByIdAsync(Arg.Is<GuidObject>(f
                => f.Value.Equals(_command.Id)), default);

        await _diagnosisRepositoryMock.Received(1)
            .DeleteAsync(Arg.Is<GuidObject>(f
                => f.Equals(_validDiagnosis.Id)), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailedResult_DoctorNotFound()
    {
        // arrange
        Set_DoctorMessageQueue_NotFoundFailure();

        // act
        Result result = await _handler.Handle(_command, default);

        // assert
        await _messageQeueServicesMock.Received(1)
            .GetDoctorByIdAsync(Arg.Is<Guid>(f
                => f.Equals(_command.DoctorId)), default);

        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailedResult_DoctorNullValue()
    {
        // arrange
        Set_DoctorMessageQueue_NullValueFailure();

        // act
        Result result = await _handler.Handle(_command, default);

        // assert
        await _messageQeueServicesMock.Received(1)
            .GetDoctorByIdAsync(Arg.Is<Guid>(f
                => f.Equals(_command.DoctorId)), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NullValue);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailedResult_DiagnosisByIdNotFound()
    {
        // arrange
        Set_DoctorMessageQueue_Succes();
        Set_GetDiagnosisById_NotFound();

        // act
        Result result = await _handler.Handle(_command, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailedResult_DiagnosisDoctorIdIsNotTheOwner()
    {
        // arrange
        Set_DoctorMessageQueue_Succes();
        Set_GetDiagnosisById_Success_WithOtherOwner();

        // act
        Result result = await _handler.Handle(_command, default);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}
