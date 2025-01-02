using Bogus;
using NSubstitute;
using Service.Diagnoses.Domain.Entities;
using Shared.Common.Helper.ErrorsHandler;
using Service.Diagnoses.Domain.Abstractions;
using Shared.Message.Queue.Requests.Responses;

namespace Services.Diagnoses.Application.UnitTests;

public abstract class BaseTestSharedConfiguration
{
    protected readonly Faker _faker;

    protected readonly IDiagnosisRepository _diagnosisRepositoryMock;
    protected readonly IMessageQeueServices _messageQeueServicesMock;

    protected readonly DoctorQueueResponse _validDoctor;
    protected readonly PatientQueueResponse _validPatient;

    protected BaseTestSharedConfiguration()
    {
        _faker = new();
        _diagnosisRepositoryMock = Substitute.For<IDiagnosisRepository>();
        _messageQeueServicesMock = Substitute.For<IMessageQeueServices>();

        _validDoctor = DoctorQueueResponse.Map(
            Guid.NewGuid(),
            _faker.Person.FullName,
            _faker.Internet.UserName(),
            _faker.Random.Number(60),
            DateTimeOffset.UtcNow);

        _validPatient = PatientQueueResponse.Map(
            Guid.NewGuid(),
            _faker.Person.FullName,
            _faker.Random.Number(80),
            DateTimeOffset.UtcNow);
    }

    #region Doctor Message Queue

    public void Set_DoctorMessageQueue_Succes()
        => _messageQeueServicesMock.GetDoctorByIdAsync(
                        Arg.Any<Guid>(),
                        Arg.Any<CancellationToken>())
                    .Returns(_validDoctor);

    public void Set_DoctorMessageQueue_NotFoundFailure()
        => _messageQeueServicesMock.GetDoctorByIdAsync(
                        Arg.Any<Guid>(),
                        Arg.Any<CancellationToken>())
                    .Returns(
                        Result.Failure<DoctorQueueResponse>(
                            Error.NotFound("doctorNotFound", "The doctor was no found")));

    public void Set_DoctorMessageQueue_NullValueFailure()
        => _messageQeueServicesMock.GetDoctorByIdAsync(
                        Arg.Any<Guid>(),
                        Arg.Any<CancellationToken>())
                    .Returns(
                        Result.Failure<DoctorQueueResponse>(
                            Error.NullValue));

    #endregion

    #region Patient Message Queue

    public void Set_PatientMessageQueue_Succes()
        => _messageQeueServicesMock.GetPatientByIdAsync(
                        Arg.Any<Guid>(),
                        Arg.Any<CancellationToken>())
                    .Returns(_validPatient);

    public void Set_PatientMessageQueue_NotFoundFailure()
        => _messageQeueServicesMock.GetPatientByIdAsync(
                        Arg.Any<Guid>(),
                        Arg.Any<CancellationToken>())
                    .Returns(
                        Result.Failure<PatientQueueResponse>(
                            Error.NotFound("patientNotFound", "The patient was no found")));

    public void Set_PatientMessageQueue_NullValueFailure()
        => _messageQeueServicesMock.GetPatientByIdAsync(
                        Arg.Any<Guid>(),
                        Arg.Any<CancellationToken>())
                    .Returns(
                        Result.Failure<PatientQueueResponse>(
                            Error.NullValue));
    #endregion

    #region Diagnoses Repository
    
    public void Set_CreateDiagnoses_Success()
        => _diagnosisRepositoryMock.CreateAsync(
                        Arg.Any<Diagnosis>(),
                        Arg.Any<CancellationToken>())
                    .Returns(Task.CompletedTask);

    #endregion
}
