using Services.Doctors.Domain.Dtos;
using Shared.Common.Helper.ErrorsHandler;
using Services.Doctors.Domain.Abstractions;
using Value.Objects.Helper.Values.Primitives;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Doctors.Application.UseCases;

internal sealed class GetDoctorCollectionByNameQueryHandler
    : IQueryHandler<GetDoctorCollectionByNameQuery, IEnumerable<DoctorDto>>
{
    private readonly IDoctorRepository _doctorRepository;

    public GetDoctorCollectionByNameQueryHandler(IDoctorRepository doctorRepository)
    {
        ArgumentNullException.ThrowIfNull(doctorRepository, nameof(doctorRepository));

        _doctorRepository = doctorRepository;
    }

    public async Task<Result<IEnumerable<DoctorDto>>> Handle(GetDoctorCollectionByNameQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<DoctorDto> collection = await _doctorRepository.CollectionByNameAsync(
            StringObject.Create(request.Name), 
            request.PageNumber, 
            cancellationToken);

        return Result.Success(collection.AsEnumerable());
    }
}
