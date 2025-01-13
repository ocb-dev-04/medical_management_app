using Bogus;
using NSubstitute;
using Service.Diagnoses.Domain.Entities;
using Shared.Common.Helper.ErrorsHandler;
using Service.Diagnoses.Domain.Abstractions;
using Shared.Message.Queue.Requests.Responses;
using Service.Diagnoses.Domain.Enums;
using Value.Objects.Helper.Values.Primitives;
using Service.Diagnoses.Domain.Errors;

namespace Services.Diagnoses.Application.UnitTests;

public abstract class BaseTestSharedConfiguration
{
    protected readonly Faker _faker;

    protected readonly IDiagnosisRepository _diagnosisRepositoryMock;
    protected readonly IMessageQeueServices _messageQeueServicesMock;

    protected readonly DoctorQueueResponse _validDoctor;
    protected readonly PatientQueueResponse _validPatient;
    protected readonly Diagnosis _validDiagnosis;
    protected readonly Diagnosis _validDiagnosisWithOtherOwner;

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

        _validDiagnosis = Diagnosis.Create(
            GuidObject.Create(_validDoctor.Id.ToString()),
            GuidObject.Create(_validPatient.Id.ToString()),
            StringObject.Create(_faker.Commerce.ProductName()),
            StringObject.Create(_faker.Commerce.ProductDescription()),
            StringObject.Create(_faker.Lorem.Paragraph(240)),
            DosageIntervals.EverySixHours.ToTimeSpan());

        _validDiagnosisWithOtherOwner = Diagnosis.Create(
            GuidObject.New(),
            GuidObject.Create(_validPatient.Id.ToString()),
            StringObject.Create(_faker.Commerce.ProductName()),
            StringObject.Create(_faker.Commerce.ProductDescription()),
            StringObject.Create(_faker.Lorem.Paragraph(240)),
            DosageIntervals.EverySixHours.ToTimeSpan());
    }

    #region Doctor Message Queue

    public void Set_DoctorMessageQueue_Succes()
        => _messageQeueServicesMock.GetDoctorByIdAsync(
                        _validDoctor.Id,
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
                        _validPatient.Id,
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
    
    public void Set_GetDiagnosisById_Success()
        => _diagnosisRepositoryMock.ByIdAsync(
                        _validDiagnosis.Id,
                        Arg.Any<CancellationToken>())
                    .Returns(_validDiagnosis);

    public void Set_GetDiagnosisById_Success_WithOtherOwner()
        => _diagnosisRepositoryMock.ByIdAsync(
                        Arg.Any<GuidObject>(),
                        //_validDiagnosisWithOtherOwner.Id,
                        Arg.Any<CancellationToken>())
                    .Returns(_validDiagnosisWithOtherOwner);

    public void Set_GetDiagnosisById_NotFound()
        => _diagnosisRepositoryMock.ByIdAsync(
                        Arg.Any<GuidObject>(),
                        Arg.Any<CancellationToken>())
                    .Returns(
                        Result.Failure<Diagnosis>(
                            DiagnosisErrors.NotFound));
    
    public void Set_CreateDiagnosis_Success()
        => _diagnosisRepositoryMock.CreateAsync(
                        Arg.Any<Diagnosis>(),
                        Arg.Any<CancellationToken>())
                    .Returns(Task.CompletedTask);

    #endregion
}
