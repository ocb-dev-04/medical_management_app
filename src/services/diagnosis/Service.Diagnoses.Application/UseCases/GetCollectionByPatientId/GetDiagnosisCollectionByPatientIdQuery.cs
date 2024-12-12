using FluentValidation;
using Shared.Domain.Constants;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Service.Diagnoses.Application.UseCases;

public sealed record GetDiagnosisCollectionByPatientIdQuery(Guid Id, int PageNumber) 
    : IQuery<IEnumerable<DiagnosisResponse>>;

internal sealed class GetDiagnosisCollectionByPatientIdQueryValidator
    : AbstractValidator<GetDiagnosisCollectionByPatientIdQuery>
{
    public GetDiagnosisCollectionByPatientIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField);

        RuleFor(x => x.PageNumber)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField)
        .GreaterThan(0)
            .WithMessage(ValidationConstants.CantBeNegativeOrZero);
    }
}