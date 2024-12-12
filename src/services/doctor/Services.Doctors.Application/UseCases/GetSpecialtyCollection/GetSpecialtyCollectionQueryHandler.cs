using Shared.Common.Helper.ErrorsHandler;
using Services.Doctors.Domain.Abstractions;
using Value.Objects.Helper.Values.Primitives;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Doctors.Application.UseCases;

internal sealed class GetSpecialtyCollectionQueryHandler
    : IQueryHandler<GetSpecialtyCollectionQuery, IEnumerable<string>>
{
    private readonly IDoctorRepository _doctorRepository;

    public GetSpecialtyCollectionQueryHandler(IDoctorRepository doctorRepository)
    {
        ArgumentNullException.ThrowIfNull(doctorRepository, nameof(doctorRepository));

        _doctorRepository = doctorRepository;
    }

    public async Task<Result<IEnumerable<string>>> Handle(GetSpecialtyCollectionQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<StringObject> collection = await _doctorRepository.SpecialtyCollectionAsync(cancellationToken);
        IEnumerable<string> mapped = collection.Select(s => s.Value);

        return Result.Success(mapped);
    }
}
