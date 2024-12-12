using FluentValidation;
using Shared.Domain.Constants;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Service.Diagnoses.Application.UseCases;

public sealed record GetDiagnosisByIdQuery(Guid Id)
    : IQuery<DiagnosisResponse>;

internal sealed class GetDiagnosisByIdQueryValidator
    : AbstractValidator<GetDiagnosisByIdQuery>
{
    public GetDiagnosisByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField);
    }
}