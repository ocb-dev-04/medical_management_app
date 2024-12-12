using FluentValidation;
using Shared.Domain.Constants;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Auth.Application.UseCases;

public sealed record SignupCommand(string Email, string Password) 
    : ICommand<SignupResponse>;

internal sealed class SignupCommandValidator
    : AbstractValidator<SignupCommand>
{
    public SignupCommandValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Continue)
            .NotEmpty()
                .WithMessage(ValidationConstants.FieldCantBeEmpty)
            .NotNull()
                .WithMessage(ValidationConstants.RequiredField)
            .EmailAddress()
                .WithMessage(ValidationConstants.InvalidEmail)
            .MaximumLength(100)
                .WithMessage(ValidationConstants.LongField);

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Continue)
            .NotEmpty()
                .WithMessage(ValidationConstants.FieldCantBeEmpty)
            .NotNull()
                .WithMessage(ValidationConstants.RequiredField)
            .MinimumLength(6)
                .WithMessage(ValidationConstants.ShortField)
            .MaximumLength(64)
                .WithMessage(ValidationConstants.LongField)
            .Matches("[a-z]")
                .WithMessage(ValidationConstants.LowercaseLetterRequired)
            .Matches("[0-9]")
                .WithMessage(ValidationConstants.DigitRequired)
            .Matches("[A-Z]")
                .WithMessage(ValidationConstants.UppercaseLetterRequired)
            .Matches("[^a-zA-Z0-9]")
                .WithMessage(ValidationConstants.PasswordSpecialCharacterRequired);
    }
}