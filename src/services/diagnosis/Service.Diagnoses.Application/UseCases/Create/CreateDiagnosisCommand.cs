using FluentValidation;
using Shared.Domain.Constants;
using Service.Diagnoses.Domain.Enums;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Service.Diagnoses.Application.UseCases;

public sealed record CreateDiagnosisCommand(
    Guid DoctorId,
    Guid PatientId,
    string Disease,
    string Medicine,
    string Indications,
    DosageIntervals DosageInterval) : ICommand<DiagnosisResponse>;

internal sealed class CreateDiagnosisCommandValidator
    : AbstractValidator<CreateDiagnosisCommand>
{
    private static readonly string _invalidDosageInterval = "invalidDosageInterval";

    public CreateDiagnosisCommandValidator()
    {
        RuleFor(x => x.DoctorId)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField);

        RuleFor(x => x.PatientId)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField);

        RuleFor(x => x.Disease)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField)
        .MaximumLength(100)
            .WithMessage(ValidationConstants.LongField);

        RuleFor(x => x.Medicine)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField)
        .MaximumLength(100)
            .WithMessage(ValidationConstants.LongField);

        RuleFor(x => x.Indications)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField)
        .MaximumLength(250)
            .WithMessage(ValidationConstants.LongField);

        RuleFor(x => x.DosageInterval)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField)
        .Must(value => Enum.IsDefined(typeof(DosageIntervals), value))
            .WithMessage(_invalidDosageInterval);
    }
}