using FluentAssertions;
using Service.Diagnoses.Domain.Enums;
using Shared.Common.Helper.ErrorsHandler;
using Service.Diagnoses.Application.UseCases;

namespace Services.Diagnoses.Application.UnitTests.UseCases.Create;

public class CreateDiagnosisCommandHandlerTest
    : BaseTestSharedConfiguration
{
    private readonly CreateDiagnosisCommand _command;
    private readonly CreateDiagnosisCommandHandler _handler;

    public CreateDiagnosisCommandHandlerTest()
    {
        _command = new(
            _validDoctor.Id,
            _validPatient.Id,
            _faker.Commerce.ProductName(),
            _faker.Commerce.ProductDescription(),
            _faker.Lorem.Paragraph(240),
            DosageIntervals.EverySixHours);

        _handler = new(
            _diagnosisRepositoryMock,
            _messageQeueServicesMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult()
    {
        // arrange
        Set_DoctorMessageQueue_Succes();
        Set_PatientMessageQueue_Succes();

        // act
       Result<DiagnosisResponse> result = await _handler.Handle(_command, default);

        // assert
        result.IsSuccess.Should().BeTrue();
    }
}
