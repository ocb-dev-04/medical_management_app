using FluentValidation;
using Shared.Domain.Constants;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Patients.Application.UseCases;

public sealed record RemovePatientCommand(Guid Id, Guid DoctorId)
    : ICommand;

internal sealed class RemovePatientCommandValidator
    : AbstractValidator<RemovePatientCommand>
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