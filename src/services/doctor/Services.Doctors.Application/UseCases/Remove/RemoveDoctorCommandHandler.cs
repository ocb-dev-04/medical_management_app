using Services.Doctors.Domain.Entities;
using Shared.Common.Helper.ErrorsHandler;
using Services.Doctors.Domain.Abstractions;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using Services.Doctors.Domain.Errors;
using Services.Doctors.Domain.StrongIds;
using Services.Doctors.Application.Services;
using Shared.Message.Queue.Requests;
using Services.Doctors.Domain.Dtos;
using Shared.Domain.Abstractions.Services;
using Shared.Domain.Constants;

namespace Services.Doctors.Application.UseCases;

internal sealed class RemoveDoctorCommandHandler
    : ICommandHandler<RemoveDoctorCommand>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly MessageQeueServices _messageQeueServices;
    private readonly IElasticSearchService<DoctorDto> _doctorSearchClient;

    public RemoveDoctorCommandHandler(
        IDoctorRepository doctorRepository,
        MessageQeueServices messageQeueServices,
        IElasticSearchService<DoctorDto> doctorSearchClient)
    {
        ArgumentNullException.ThrowIfNull(doctorRepository, nameof(doctorRepository));
        ArgumentNullException.ThrowIfNull(messageQeueServices, nameof(messageQeueServices));
        ArgumentNullException.ThrowIfNull(doctorSearchClient, nameof(doctorSearchClient));

        _doctorRepository = doctorRepository;
        _messageQeueServices = messageQeueServices;
        _doctorSearchClient = doctorSearchClient;
    }

    public async Task<Result> Handle(RemoveDoctorCommand request, CancellationToken cancellationToken)
    {
        Result<DoctorId> doctorId = DoctorId.Create(request.Id);
        if (doctorId.IsFailure)
            return Result.Failure(doctorId.Error);

        Result<Doctor> found = await _doctorRepository.ByIdAsync(doctorId.Value, cancellationToken);
        if (found.IsFailure)
            return Result.Failure(found.Error);

        Result<CredentialQueueResponse> checkCredential = await _messageQeueServices.GetCredentialByIdAsync(request.CredentialId, cancellationToken);
        if (checkCredential.IsFailure)
            return Result.Failure<DoctorResponse>(checkCredential.Error);

        if (!found.Value.CredentialId.Value.Equals(request.CredentialId))
            return Result.Failure(DoctorErrors.YouAreNotTheOwner);

        await _doctorRepository.DeleteAsync(found.Value.Id, cancellationToken);
        await _doctorRepository.CommitAsync(cancellationToken);

        await _doctorSearchClient.RemoveAsync(
            found.Value.Id.Value.ToString(),
            ServicesConstants.ElasticSearchDoctorIndex,
            cancellationToken);

        return Result.Success();
    }
}
