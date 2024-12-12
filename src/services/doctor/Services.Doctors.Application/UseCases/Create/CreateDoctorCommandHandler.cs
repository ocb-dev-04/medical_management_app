using Shared.Common.Helper.ErrorsHandler;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using Services.Doctors.Domain.Abstractions;
using Value.Objects.Helper.Values.Primitives;
using Services.Doctors.Domain.Errors;
using Services.Doctors.Domain.Entities;
using Services.Doctors.Application.Services;
using Shared.Message.Queue.Requests;

namespace Services.Doctors.Application.UseCases;

internal sealed class CreateDoctorCommandHandler
    : ICommandHandler<CreateDoctorCommand, DoctorResponse>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly MessageQeueServices _messageQeueServices;

    public CreateDoctorCommandHandler(
        IDoctorRepository doctorRepository,
        MessageQeueServices messageQeueServices)
    {
        ArgumentNullException.ThrowIfNull(doctorRepository, nameof(doctorRepository));
        ArgumentNullException.ThrowIfNull(messageQeueServices, nameof(messageQeueServices));

        _doctorRepository = doctorRepository;
        _messageQeueServices = messageQeueServices;
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

        return DoctorResponse.Map(created);
    }
}
