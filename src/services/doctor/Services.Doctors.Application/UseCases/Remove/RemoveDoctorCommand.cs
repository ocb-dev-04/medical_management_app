using FluentValidation;
using Shared.Domain.Constants;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Doctors.Application.UseCases;

public sealed record RemoveDoctorCommand(Guid Id, Guid CredentialId) 
    : ICommand;

internal sealed class RemoveDoctorCommandValidator
    : AbstractValidator<RemoveDoctorCommand>
{
    public RemoveDoctorCommandValidator()
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField);

        RuleFor(x => x.CredentialId)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField);

    }
}