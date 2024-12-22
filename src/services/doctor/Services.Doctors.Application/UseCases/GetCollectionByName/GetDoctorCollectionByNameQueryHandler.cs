using Services.Doctors.Domain.Dtos;
using Shared.Common.Helper.ErrorsHandler;
using Shared.Domain.Abstractions.Services;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using Shared.Domain.Constants;

namespace Services.Doctors.Application.UseCases;

internal sealed class GetDoctorCollectionByNameQueryHandler
    : IQueryHandler<GetDoctorCollectionByNameQuery, IEnumerable<DoctorDto>>
{
    private readonly IElasticSearchService<DoctorDto> _doctorSearchClient;

    public GetDoctorCollectionByNameQueryHandler(
        IElasticSearchService<DoctorDto> doctorSearchClient)
    {
        ArgumentNullException.ThrowIfNull(doctorSearchClient, nameof(doctorSearchClient));

        _doctorSearchClient = doctorSearchClient;
    }

    public async Task<Result<IEnumerable<DoctorDto>>> Handle(GetDoctorCollectionByNameQuery request, CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<DoctorDto>> collection = await _doctorSearchClient.SearchAsync(
            f => f.Name,
            request.Name,
            ServicesConstants.ElasticSearchDoctorIndex,
            cancellationToken);

        if (collection.IsFailure)
            return Result.Failure<IEnumerable<DoctorDto>>(collection.Error);

        return Result.Success(collection.Value.AsEnumerable());
    }
}
