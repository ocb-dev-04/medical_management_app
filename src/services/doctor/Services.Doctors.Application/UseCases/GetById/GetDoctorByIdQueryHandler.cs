using Shared.Common.Helper.ErrorsHandler;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using Services.Doctors.Domain.Abstractions;
using Services.Doctors.Domain.StrongIds;
using Services.Doctors.Domain.Entities;

namespace Services.Doctors.Application.UseCases;

internal sealed class GetDoctorByIdQueryHandler
    : IQueryHandler<GetDoctorByIdQuery, DoctorResponse>
{
    private readonly IDoctorRepository _doctorRepository;

    public GetDoctorByIdQueryHandler(IDoctorRepository doctorRepository)
    {
        ArgumentNullException.ThrowIfNull(doctorRepository, nameof(doctorRepository));

        _doctorRepository = doctorRepository;
    }

    public async Task<Result<DoctorResponse>> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        Result<DoctorId> doctorId = DoctorId.Create(request.Id);
        if (doctorId.IsFailure)
            return Result.Failure<DoctorResponse>(doctorId.Error);

        Result<Doctor> found = await _doctorRepository.ByIdAsync(doctorId.Value, cancellationToken);
        if(found.IsFailure)
            return Result.Failure<DoctorResponse>(found.Error);

        return DoctorResponse.Map(found.Value);
    }
}
