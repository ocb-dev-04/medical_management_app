using Shared.Common.Helper.ErrorsHandler;
using Services.Patients.Domain.Abstractions;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using Services.Patients.Application.Services;
using Services.Patients.Domain.Entities;
using Services.Patients.Domain.Errors;
using Services.Patients.Domain.StrongIds;
using Shared.Message.Queue.Requests.Responses;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Patients.Application.UseCases;

internal sealed class RemovePatientCommandHandler
    : ICommandHandler<RemovePatientCommand>
{
    private readonly IPatientRepository _patientRepository;
    private readonly MessageQeueServices _messageQeueServices;

    public RemovePatientCommandHandler(
        IPatientRepository patientRepository,
        MessageQeueServices messageQeueServices)
    {
        ArgumentNullException.ThrowIfNull(patientRepository, nameof(patientRepository));
        ArgumentNullException.ThrowIfNull(messageQeueServices, nameof(messageQeueServices));

        _patientRepository = patientRepository;
        _messageQeueServices = messageQeueServices;
    }

    public async Task<Result> Handle(RemovePatientCommand request, CancellationToken cancellationToken)
    {
        Result<PatientId> patientId = PatientId.Create(request.Id);
        if (patientId.IsFailure)
            return Result.Failure(patientId.Error);

        Result<Patient> found = await _patientRepository.ByIdAsync(patientId.Value, cancellationToken);
        if (found.IsFailure)
            return Result.Failure(found.Error);

        GuidObject doctorId = GuidObject.Create(request.DoctorId.ToString());
        Result<DoctorQueueResponse> checkDoctor = await _messageQeueServices.GetDoctorByIdAsync(doctorId.Value, cancellationToken);
        if (checkDoctor.IsFailure)
            return Result.Failure(checkDoctor.Error);

        if (!found.Value.DoctorId.Equals(doctorId))
            return Result.Failure(PatientErrors.YouAreNotTheOwner);

        found.Value.SetAsDeleted();
        await _patientRepository.CommitAsync(cancellationToken);

        return Result.Success();

    }
}
