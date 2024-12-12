using Service.Diagnoses.Domain.Enums;
using Shared.Common.Helper.ErrorsHandler;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Service.Diagnoses.Application.UseCases;

internal sealed class GetDosageIntervalCollectionQueryHandler
    : IQueryHandler<GetDosageIntervalCollectionQuery, IEnumerable<DosageIntervalResponse>>
{
    public async Task<Result<IEnumerable<DosageIntervalResponse>>> Handle(GetDosageIntervalCollectionQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<DosageIntervalResponse> collection = Enum.GetValues(typeof(DosageIntervals))
            .Cast<DosageIntervals>()
            .Select(e
                => new DosageIntervalResponse(char.ToLower(e.ToString()[0]) + e.ToString()[1..], (int)e))
            .AsEnumerable();

        return Result.Success(collection);
    }
}
