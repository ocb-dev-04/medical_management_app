using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Service.Diagnoses.Application.UseCases;

public sealed record GetDosageIntervalCollectionQuery() 
    : IQuery<IEnumerable<DosageIntervalResponse>>;