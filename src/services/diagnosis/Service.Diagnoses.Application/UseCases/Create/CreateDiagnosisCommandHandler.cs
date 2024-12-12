using Service.Diagnoses.Domain.Enums;
using Service.Diagnoses.Domain.Entities;
using Shared.Common.Helper.ErrorsHandler;
using Service.Diagnoses.Domain.Abstractions;
using Value.Objects.Helper.Values.Primitives;
using Service.Diagnoses.Application.Services;
using Shared.Message.Queue.Requests.Responses;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Service.Diagnoses.Application.UseCases;

internal sealed class CreateDiagnosisCommandHandler
    : ICommandHandler<CreateDiagnosisCommand, DiagnosisResponse>
{
    private readonly IDiagnosisRepository _diagnosisRepository;
    private readonly MessageQeueServices _messageQeueServices;

    public CreateDiagnosisCommandHandler(
        IDiagnosisRepository diagnosisRepository,
        MessageQeueServices messageQeueServices)
    {
        ArgumentNullException.ThrowIfNull(diagnosisRepository, nameof(diagnosisRepository));
        ArgumentNullException.ThrowIfNull(messageQeueServices, nameof(messageQeueServices));

        _diagnosisRepository = diagnosisRepository;
        _messageQeueServices = messageQeueServices;
    }

    public async Task<Result<DiagnosisResponse>> Handle(CreateDiagnosisCommand request, CancellationToken cancellationToken)
    {
        Result<DoctorQueueResponse> checkDoctor = await _messageQeueServices.GetDoctorByIdAsync(request.DoctorId, cancellationToken);
        if (checkDoctor.IsFailure)
            return Result.Failure<DiagnosisResponse>(checkDoctor.Error);
        
        Result<PatientQueueResponse> checkPatient = await _messageQeueServices.GetPatientByIdAsync(request.PatientId, cancellationToken);
        if (checkPatient.IsFailure)
            return Result.Failure<DiagnosisResponse>(checkPatient.Error);

        Diagnosis created = Diagnosis.Create(
            GuidObject.Create(request.DoctorId.ToString()),
            GuidObject.Create(request.PatientId.ToString()),
            StringObject.Create(request.Disease),
            StringObject.Create(request.Medicine),
            StringObject.Create(request.Indications),
            request.DosageInterval.ToTimeSpan());

        await _diagnosisRepository.CreateAsync(created, cancellationToken);
        _diagnosisRepository.Commit();

        return DiagnosisResponse.Map(created);
    }
}
