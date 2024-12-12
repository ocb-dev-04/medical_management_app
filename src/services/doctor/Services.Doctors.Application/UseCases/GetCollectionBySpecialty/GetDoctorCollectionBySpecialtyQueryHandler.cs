using Services.Doctors.Domain.Dtos;
using Shared.Common.Helper.ErrorsHandler;
using Services.Doctors.Domain.Abstractions;
using Value.Objects.Helper.Values.Primitives;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Doctors.Application.UseCases;

internal sealed class GetDoctorCollectionBySpecialtyQueryHandler
    : IQueryHandler<GetDoctorCollectionBySpecialtyQuery, IEnumerable<DoctorDto>>
{
    private readonly IDoctorRepository _doctorRepository;

    public GetDoctorCollectionBySpecialtyQueryHandler(IDoctorRepository doctorRepository)
    {
        ArgumentNullException.ThrowIfNull(doctorRepository, nameof(doctorRepository));

        _doctorRepository = doctorRepository;
    }

    public async Task<Result<IEnumerable<DoctorDto>>> Handle(GetDoctorCollectionBySpecialtyQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<DoctorDto> collection = await _doctorRepository.CollectionBySpecialtyAsync(
            StringObject.Create(request.Specialty), 
            request.PageNumber, 
            cancellationToken);

        return Result.Success(collection.AsEnumerable());
    }
}
