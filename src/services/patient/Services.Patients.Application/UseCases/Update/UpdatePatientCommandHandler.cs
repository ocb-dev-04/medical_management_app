using Services.Patients.Domain.Errors;
using Services.Patients.Domain.Entities;
using Shared.Common.Helper.ErrorsHandler;
using Services.Patients.Domain.StrongIds;
using Services.Patients.Domain.Abstractions;
using Value.Objects.Helper.Values.Primitives;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using Services.Patients.Application.Services;
using Shared.Message.Queue.Requests.Responses;

namespace Services.Patients.Application.UseCases;

internal sealed class UpdatePatientCommandHandler
    : ICommandHandler<UpdatePatientCommand, PatientResponse>
{
    private readonly IPatientRepository _patientRepository;
    private readonly MessageQeueServices _messageQeueServices;

    public UpdatePatientCommandHandler(
        IPatientRepository patientRepository,
        MessageQeueServices messageQeueServices)
    {
        ArgumentNullException.ThrowIfNull(patientRepository, nameof(patientRepository));
        ArgumentNullException.ThrowIfNull(messageQeueServices, nameof(messageQeueServices));

        _patientRepository = patientRepository;
        _messageQeueServices = messageQeueServices;
    }

    public async Task<Result<PatientResponse>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        Result<PatientId> patientId = PatientId.Create(request.Id);
        if (patientId.IsFailure)
            return Result.Failure<PatientResponse>(patientId.Error);

        Result<Patient> found = await _patientRepository.ByIdAsync(patientId.Value, cancellationToken);
        if (found.IsFailure)
            return Result.Failure<PatientResponse>(found.Error);

        GuidObject doctorId = GuidObject.Create(request.Body.DoctorId.ToString());
        Result<DoctorQueueResponse> checkDoctor = await _messageQeueServices.GetDoctorByIdAsync(doctorId.Value, cancellationToken);
        if (checkDoctor.IsFailure)
            return Result.Failure<PatientResponse>(checkDoctor.Error);

        if (!found.Value.DoctorId.Equals(doctorId))
            return Result.Failure<PatientResponse>(PatientErrors.YouAreNotTheOwner);

        found.Value.UpdateGeneralData(
            StringObject.Create(request.Body.Name),
            IntegerObject.Create(request.Body.Age));
        await _patientRepository.CommitAsync(cancellationToken);

        return PatientResponse.Map(found.Value);
    }
}
