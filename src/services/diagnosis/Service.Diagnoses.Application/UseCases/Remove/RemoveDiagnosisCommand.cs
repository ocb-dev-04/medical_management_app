using FluentValidation;
using Shared.Domain.Constants;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Service.Diagnoses.Application.UseCases;

public sealed record RemoveDiagnosisCommand(Guid Id, Guid DoctorId)
    : ICommand;

internal sealed class RemovePatientCommandValidator
    : AbstractValidator<RemoveDiagnosisCommand>
{
    public RemovePatientCommandValidator()
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField);

        RuleFor(x => x.DoctorId)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField);
    }
}