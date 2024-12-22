using Shared.Message.Queue.Requests;
using Services.Doctors.Domain.Errors;
using Services.Doctors.Domain.Entities;
using Services.Doctors.Domain.StrongIds;
using Shared.Common.Helper.ErrorsHandler;
using Services.Doctors.Domain.Abstractions;
using Services.Doctors.Application.Services;
using Value.Objects.Helper.Values.Primitives;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using Services.Doctors.Domain.Dtos;
using Shared.Domain.Constants;
using Shared.Domain.Abstractions.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Services.Doctors.Application.UseCases;

internal sealed class UpdateDoctorCommandHandler
    : ICommandHandler<UpdateDoctorCommand, DoctorResponse>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly MessageQeueServices _messageQeueServices;
    private readonly IElasticSearchService<DoctorDto> _doctorSearchClient;

    public UpdateDoctorCommandHandler(
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

    public async Task<Result<DoctorResponse>> Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
    {
        Result<DoctorId> doctorId = DoctorId.Create(request.Id); 
        if (doctorId.IsFailure)
            return Result.Failure<DoctorResponse>(doctorId.Error);

        Result<Doctor> found = await _doctorRepository.ByIdAsync(doctorId.Value, cancellationToken);
        if (found.IsFailure)
            return Result.Failure<DoctorResponse>(found.Error);

        Result<CredentialQueueResponse> checkCredential = await _messageQeueServices.GetCredentialByIdAsync(request.CredentialId, cancellationToken);
        if (checkCredential.IsFailure)
            return Result.Failure<DoctorResponse>(checkCredential.Error);

        if (!found.Value.CredentialId.Value.Equals(request.CredentialId))
            return Result.Failure<DoctorResponse>(DoctorErrors.YouAreNotTheOwner);

        found.Value.UpdateGeneaalData(
            StringObject.Create(request.Body.Name),
            StringObject.Create(request.Body.Specialty),
            IntegerObject.Create(request.Body.ExperienceInYears));
        await _doctorRepository.CommitAsync(cancellationToken);

        await _doctorSearchClient.AddOrUpdateAsync(
            found.Value.Id.Value.ToString(),
            DoctorDto.Create(found.Value.Id, found.Value.Name, found.Value.Specialty, found.Value.ExperienceInYears, found.Value.AuditDates),
            ServicesConstants.ElasticSearchDoctorIndex,
            cancellationToken);

        return DoctorResponse.Map(found.Value);
    }
}
