using Shared.Domain.Constants;
using Services.Doctors.Domain.Dtos;
using Shared.Common.Helper.ErrorsHandler;
using Shared.Domain.Abstractions.Services;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Doctors.Application.UseCases;

internal sealed class GetDoctorCollectionBySpecialtyQueryHandler
    : IQueryHandler<GetDoctorCollectionBySpecialtyQuery, IEnumerable<DoctorDto>>
{
    private readonly IElasticSearchService<DoctorDto> _doctorSearchClient;

    public GetDoctorCollectionBySpecialtyQueryHandler(
        IElasticSearchService<DoctorDto> doctorSearchClient)
    {
        ArgumentNullException.ThrowIfNull(doctorSearchClient, nameof(doctorSearchClient));

        _doctorSearchClient = doctorSearchClient;
    }

    public async Task<Result<IEnumerable<DoctorDto>>> Handle(GetDoctorCollectionBySpecialtyQuery request, CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<DoctorDto>> collection = await _doctorSearchClient.SearchAsync(
            f => f.Specialty,
            request.Specialty,
            ServicesConstants.ElasticSearchDoctorIndex,
            cancellationToken);

        if (collection.IsFailure)
            return Result.Failure<IEnumerable<DoctorDto>>(collection.Error);

        return Result.Success(collection.Value.AsEnumerable());
    }
}
