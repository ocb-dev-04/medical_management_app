using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Doctors.Application.UseCases;

public sealed record GetSpecialtyCollectionQuery() 
    : IQuery<IEnumerable<string>>;