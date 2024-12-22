using Services.Doctors.Domain.Dtos;
using Shared.Common.Helper.ErrorsHandler;
using Shared.Domain.Abstractions.Services;
using Services.Doctors.Domain.Abstractions;
using Value.Objects.Helper.Values.Primitives;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Doctors.Application.UseCases;

internal sealed class GetDoctorCollectionBySpecialtyQueryHandler
    : IQueryHandler<GetDoctorCollectionBySpecialtyQuery, IEnumerable<DoctorDto>>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IElasticSearchService<DoctorDto> _doctorSearchClient;

    public GetDoctorCollectionBySpecialtyQueryHandler(
        IDoctorRepository doctorRepository, 
        IElasticSearchService<DoctorDto> doctorSearchClient)
    {
        ArgumentNullException.ThrowIfNull(doctorRepository, nameof(doctorRepository));
        ArgumentNullException.ThrowIfNull(doctorSearchClient, nameof(doctorSearchClient));

        _doctorRepository = doctorRepository;
        _doctorSearchClient = doctorSearchClient;
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
