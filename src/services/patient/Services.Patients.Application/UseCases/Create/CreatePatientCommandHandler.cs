using Services.Patients.Domain.Errors;
using Services.Patients.Domain.Entities;
using Shared.Common.Helper.ErrorsHandler;
using Services.Patients.Domain.Abstractions;
using Value.Objects.Helper.Values.Primitives;
using Services.Patients.Application.Services;
using Shared.Message.Queue.Requests.Responses;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Patients.Application.UseCases;

internal sealed class CreatePatientCommandHandler
    : ICommandHandler<CreatePatientCommand, PatientResponse>
{
    private readonly IPatientRepository _patientRepository;
    private readonly MessageQeueServices _messageQeueServices;

    public CreatePatientCommandHandler(
        IPatientRepository patientRepository, 
        MessageQeueServices messageQeueServices)
    {
        ArgumentNullException.ThrowIfNull(patientRepository, nameof(patientRepository));
        ArgumentNullException.ThrowIfNull(messageQeueServices, nameof(messageQeueServices));

        _patientRepository = patientRepository;
        _messageQeueServices = messageQeueServices;
    }

    public async Task<Result<PatientResponse>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        GuidObject doctorId = GuidObject.Create(request.DoctorId.ToString());
        Result<DoctorQueueResponse> checkDoctor = await _messageQeueServices.GetDoctorByIdAsync(doctorId.Value, cancellationToken);
        if(checkDoctor.IsFailure)
            return Result.Failure<PatientResponse>(checkDoctor.Error);

        StringObject name = StringObject.Create(request.Name);
        bool exist = await _patientRepository.ExistAsync(e => e.Name.Equals(name) && e.DoctorId.Equals(doctorId), cancellationToken);
        if (exist)
            return Result.Failure<PatientResponse>(PatientErrors.AlreadyExist);

        Patient created = Patient.Create(
            doctorId, 
            name, 
            IntegerObject.Create(request.Age));
        await _patientRepository.CreateAsync(created, cancellationToken);
        await _patientRepository.CommitAsync(cancellationToken);

        return PatientResponse.Map(created);
    }
}
