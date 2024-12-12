using FluentValidation;
using Shared.Domain.Constants;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Patients.Application.UseCases;

public sealed record GetPatientCollectionByDoctorIdQuery(Guid DoctorId, int PageNumber)
    : IQuery<IEnumerable<PatientResponse>>;

internal sealed class GetPatientCollectionByDoctorIdQueryValidator
    : AbstractValidator<GetPatientCollectionByDoctorIdQuery>
{
    public GetPatientCollectionByDoctorIdQueryValidator()
    {
        RuleFor(x => x.DoctorId)
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