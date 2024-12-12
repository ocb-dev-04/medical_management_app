using Services.Patients.Domain.Entities;
using Shared.Common.Helper.ErrorsHandler;
using Services.Patients.Domain.StrongIds;
using Services.Patients.Domain.Abstractions;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Patients.Application.UseCases;

internal sealed class GetPatientByIdQueryHandler
    : IQueryHandler<GetPatientByIdQuery, PatientResponse>
{
    private readonly IPatientRepository _patientRepository;

    private readonly static Error _patientSetAsDeleted
        = Error.NotFound("patientSetAsDeleted", "The patient is set as deleted");

    public GetPatientByIdQueryHandler(IPatientRepository patientRepository)
    {
        ArgumentNullException.ThrowIfNull(patientRepository, nameof(patientRepository));

        _patientRepository = patientRepository;
    }

    public async Task<Result<PatientResponse>> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        Result<PatientId> patientId = PatientId.Create(request.Id);
        if (patientId.IsFailure)
            return Result.Failure<PatientResponse>(patientId.Error);

        Result<Patient> found = await _patientRepository.ByIdAsync(patientId.Value, cancellationToken);
        if (found.IsFailure)
            return Result.Failure<PatientResponse>(found.Error);

        if(found.Value.Deleted.Value)
            return Result.Failure<PatientResponse>(_patientSetAsDeleted);

        return PatientResponse.Map(found.Value);
    }
}
