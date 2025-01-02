﻿using FluentAssertions;
using Service.Diagnoses.Domain.Enums;
using Shared.Common.Helper.ErrorsHandler;
using Service.Diagnoses.Application.UseCases;
using NSubstitute;
using Service.Diagnoses.Domain.Entities;

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
        await _messageQeueServicesMock.Received(1)
            .GetDoctorByIdAsync(Arg.Is<Guid>(f 
                => f.Equals(_command.DoctorId)), default);

        await _messageQeueServicesMock.Received(1)
            .GetPatientByIdAsync(Arg.Is<Guid>(f 
                => f.Equals(_command.PatientId)), default);

        await _diagnosisRepositoryMock.Received(1)
            .CreateAsync(Arg.Is<Diagnosis>(f 
                => f.DoctorId.Value.Equals(_command.DoctorId) &&
                    f.PatientId.Value.Equals(_command.PatientId) &&
                    f.Disease.Value.Equals(_command.Disease) &&
                    f.Medicine.Value.Equals(_command.Medicine) &&
                    f.Indications.Value.Equals(_command.Indications) &&
                    f.DosageInterval.Equals(_command.DosageInterval.ToTimeSpan())), default);
        
        result.IsSuccess.Should().BeTrue();
    }
}
