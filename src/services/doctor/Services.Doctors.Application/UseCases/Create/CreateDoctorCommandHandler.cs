using Shared.Common.Helper.ErrorsHandler;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using Services.Doctors.Domain.Abstractions;
using Value.Objects.Helper.Values.Primitives;
using Services.Doctors.Domain.Errors;
using Services.Doctors.Domain.Entities;
using Services.Doctors.Application.Services;
using Shared.Message.Queue.Requests;
using Shared.Domain.Abstractions.Services;
using Services.Doctors.Domain.Dtos;
using Shared.Domain.Constants;

namespace Services.Doctors.Application.UseCases;

internal sealed class CreateDoctorCommandHandler
    : ICommandHandler<CreateDoctorCommand, DoctorResponse>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly MessageQeueServices _messageQeueServices;
    private readonly IElasticSearchService<DoctorDto> _doctorSearchClient;

    public CreateDoctorCommandHandler(
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

    public async Task<Result<DoctorResponse>> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        Result<CredentialQueueResponse> checkCredential = await _messageQeueServices.GetCredentialByIdAsync(request.CredentialId, cancellationToken);
        if(checkCredential.IsFailure)
            return Result.Failure<DoctorResponse>(checkCredential.Error);

        StringObject name = StringObject.Create(request.Body.Name);
        StringObject specialty = StringObject.Create(request.Body.Specialty);
        bool exist = await _doctorRepository.ExistAsync(e => e.Name.Equals(name) && e.Specialty.Equals(specialty), cancellationToken);
        if (exist)
            return Result.Failure<DoctorResponse>(DoctorErrors.AlreadyExist);

        Doctor created = Doctor.Create(
            GuidObject.Create(request.CredentialId.ToString()),
            name,
            specialty,
            IntegerObject.Create(request.Body.ExperienceInYears));

        await _doctorRepository.CreateAsync(created, cancellationToken);
        await _doctorRepository.CommitAsync(cancellationToken);

        await _doctorSearchClient.AddOrUpdateAsync(
            created.Id.Value.ToString(),
            DoctorDto.Create(created.Id, created.Name, created.Specialty, created.ExperienceInYears, created.AuditDates), 
            ServicesConstants.ElasticSearchDoctorIndex, 
            cancellationToken);

        return DoctorResponse.Map(created);
    }
}
