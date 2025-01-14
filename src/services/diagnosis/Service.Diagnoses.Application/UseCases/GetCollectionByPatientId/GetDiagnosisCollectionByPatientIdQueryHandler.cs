using Service.Diagnoses.Domain.Entities;
using Shared.Common.Helper.ErrorsHandler;
using Service.Diagnoses.Domain.Abstractions;
using Value.Objects.Helper.Values.Primitives;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Service.Diagnoses.Application.UseCases;

internal sealed class GetDiagnosisCollectionByPatientIdQueryHandler
    : IQueryHandler<GetDiagnosisCollectionByPatientIdQuery, IEnumerable<DiagnosisResponse>>
{
    private readonly IDiagnosisRepository _diagnosisRepository;

    public GetDiagnosisCollectionByPatientIdQueryHandler(IDiagnosisRepository diagnosisRepository)
    {
        ArgumentNullException.ThrowIfNull(diagnosisRepository, nameof(diagnosisRepository));

        _diagnosisRepository = diagnosisRepository;
    }

    public async Task<Result<IEnumerable<DiagnosisResponse>>> Handle(GetDiagnosisCollectionByPatientIdQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Diagnosis> collection = await _diagnosisRepository.ByPatientId(GuidObject.Create(request.Id.ToString()), request.PageNumber, cancellationToken);
        IEnumerable<DiagnosisResponse> mapped = collection.Select(s => DiagnosisResponse.Map(s));

        return Result.Success(mapped);
    }
}
