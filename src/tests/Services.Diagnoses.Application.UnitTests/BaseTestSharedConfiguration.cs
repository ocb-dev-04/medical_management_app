using Moq;
using Bogus;
using Shared.Common.Helper.ErrorsHandler;
using Service.Diagnoses.Domain.Abstractions;
using Shared.Message.Queue.Requests.Responses;
using Service.Diagnoses.Application.UseCases;
using Service.Diagnoses.Domain.Entities;

namespace Services.Diagnoses.Application.UnitTests;

public abstract class BaseTestSharedConfiguration
{
    protected readonly Faker _faker;

    protected readonly Mock<IDiagnosisRepository> _diagnosisRepositoryMock;
    protected readonly Mock<IMessageQeueServices> _messageQeueServicesMock;

    protected readonly DoctorQueueResponse _validDoctor;
    protected readonly PatientQueueResponse _validPatient;

    protected BaseTestSharedConfiguration()
    {
        _faker = new();
        _diagnosisRepositoryMock = new();
        _messageQeueServicesMock = new();

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
        => _messageQeueServicesMock.Setup(
                x => x.GetDoctorByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_validDoctor);

    public void Set_DoctorMessageQueue_NotFoundFailure()
        => _messageQeueServicesMock.Setup(
                    x => x.GetDoctorByIdAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        Result.Failure<DoctorQueueResponse>(
                            Error.NotFound("doctorNotFound", "The doctor was no found")));

    public void Set_DoctorMessageQueue_NullValueFailure()
        => _messageQeueServicesMock.Setup(
                    x => x.GetDoctorByIdAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        Result.Failure<DoctorQueueResponse>(
                            Error.NullValue));

    #endregion

    #region Patient Message Queue

    public void Set_PatientMessageQueue_Succes()
        => _messageQeueServicesMock.Setup(
                x => x.GetPatientByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_validPatient);

    public void Set_PatientMessageQueue_NotFoundFailure()
        => _messageQeueServicesMock.Setup(
                    x => x.GetPatientByIdAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        Result.Failure<PatientQueueResponse>(
                            Error.NotFound("patientNotFound", "The patient was no found")));

    public void Set_PatientMessageQueue_NullValueFailure()
        => _messageQeueServicesMock.Setup(
                    x => x.GetPatientByIdAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        Result.Failure<PatientQueueResponse>(
                            Error.NullValue));
    #endregion

    #region Diagnoses Repository
    
    public void Set_CreateDiagnoses_Success()
        => _diagnosisRepositoryMock.Setup(
                x => x.CreateAsync(
                    It.IsAny<Diagnosis>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

    #endregion
}
