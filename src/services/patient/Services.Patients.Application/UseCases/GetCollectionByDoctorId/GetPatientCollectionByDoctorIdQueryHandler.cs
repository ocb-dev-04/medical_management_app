using Services.Patients.Domain.Entities;
using Shared.Common.Helper.ErrorsHandler;
using Services.Patients.Domain.Abstractions;
using Services.Patients.Application.Services;
using Value.Objects.Helper.Values.Primitives;
using Shared.Message.Queue.Requests.Responses;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Patients.Application.UseCases;

internal sealed class GetPatientCollectionByDoctorIdQueryHandler
    : IQueryHandler<GetPatientCollectionByDoctorIdQuery, IEnumerable<PatientResponse>>
{
    private readonly IPatientRepository _patientRepository;
    private readonly MessageQeueServices _messageQeueServices;

    public GetPatientCollectionByDoctorIdQueryHandler(
        IPatientRepository patientRepository,
        MessageQeueServices messageQeueServices)
    {
        ArgumentNullException.ThrowIfNull(patientRepository, nameof(patientRepository));
        ArgumentNullException.ThrowIfNull(messageQeueServices, nameof(messageQeueServices));

        _patientRepository = patientRepository;
        _messageQeueServices = messageQeueServices;
    }

    public async Task<Result<IEnumerable<PatientResponse>>> Handle(GetPatientCollectionByDoctorIdQuery request, CancellationToken cancellationToken)
    {
        GuidObject doctorId = GuidObject.Create(request.DoctorId.ToString());
        Result<DoctorQueueResponse> checkDoctor = await _messageQeueServices.GetDoctorByIdAsync(doctorId.Value, cancellationToken);
        if (checkDoctor.IsFailure)
            return Result.Failure<IEnumerable<PatientResponse>>(checkDoctor.Error);

        IReadOnlyCollection<Patient> collection = await _patientRepository.CollectionByDoctorIdAsync(doctorId, request.PageNumber, cancellationToken);
        IEnumerable<PatientResponse> mapped = collection.Select(s => PatientResponse.Map(s));

        return Result.Success(mapped);
    }
}
