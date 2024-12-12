using CQRS.MediatR.Helper.Abstractions.Messaging;
using FluentValidation;
using Shared.Domain.Constants;

namespace Services.Patients.Application.UseCases;

public sealed record CreatePatientCommand(
    Guid DoctorId,
    string Name,
    int Age) : ICommand<PatientResponse>;

internal sealed class CreatePatientCommandValidator
    : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.DoctorId)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField);

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField)
        .MaximumLength(100)
            .WithMessage(ValidationConstants.LongField);

        RuleFor(x => x.Age)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField)
        .GreaterThan(0)
            .WithMessage(ValidationConstants.RequiredField);
    }
}