using Service.Diagnoses.Domain.Errors;
using Shared.Common.Helper.ErrorsHandler;
using Service.Diagnoses.Domain.Entities;
using Service.Diagnoses.Domain.Abstractions;
using Value.Objects.Helper.Values.Primitives;
using Service.Diagnoses.Application.Services;
using Shared.Message.Queue.Requests.Responses;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Service.Diagnoses.Application.UseCases;

internal sealed class RemoveDiagnosisCommandHandler
    : ICommandHandler<RemoveDiagnosisCommand>
{
    private readonly IDiagnosisRepository _diagnosisRepository;
    private readonly IMessageQeueServices _messageQeueServices;

    public RemoveDiagnosisCommandHandler(
        IDiagnosisRepository diagnosisRepository,
        IMessageQeueServices messageQeueServices)
    {
        ArgumentNullException.ThrowIfNull(diagnosisRepository, nameof(diagnosisRepository));
        ArgumentNullException.ThrowIfNull(messageQeueServices, nameof(messageQeueServices));

        _diagnosisRepository = diagnosisRepository;
        _messageQeueServices = messageQeueServices;
    }

    public async Task<Result> Handle(RemoveDiagnosisCommand request, CancellationToken cancellationToken)
    {
        Result<DoctorQueueResponse> checkDoctor = await _messageQeueServices.GetDoctorByIdAsync(request.DoctorId, cancellationToken);
        if (checkDoctor.IsFailure)
            return Result.Failure(checkDoctor.Error);

        Result<Diagnosis> found = await _diagnosisRepository.ByIdAsync(GuidObject.Create(request.Id.ToString()), cancellationToken);
        if (found.IsFailure)
            return Result.Failure(found.Error);

        if(!found.Value.DoctorId.Value.Equals(checkDoctor.Value.Id))
            return Result.Failure(DiagnosisErrors.YouAreNotTheOwner);

        await _diagnosisRepository.DeleteAsync(found.Value.Id, cancellationToken);
        _diagnosisRepository.Commit();

        return Result.Success();
    }
}
