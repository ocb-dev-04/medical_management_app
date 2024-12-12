using FluentValidation;
using Shared.Domain.Constants;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Patients.Application.UseCases;

public sealed record GetPatientByIdQuery(Guid Id)
    : IQuery<PatientResponse>;

internal sealed class GetPatientByIdQueryValidator
    : AbstractValidator<GetPatientByIdQuery>
{
    public GetPatientByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField);
    }
}