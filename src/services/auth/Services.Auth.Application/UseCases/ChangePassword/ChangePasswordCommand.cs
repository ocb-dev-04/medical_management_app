using FluentValidation;
using Shared.Domain.Constants;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Auth.Application.UseCases;

public sealed record ChangePasswordCommand(
    string OldPassword,
    string NewPassword,
    string ConfirmNewPassword) : ICommand;

internal sealed class ChangePasswordCommandValidator
    : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.OldPassword)
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

        RuleFor(x => x.NewPassword)
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
            .WithMessage(ValidationConstants.PasswordSpecialCharacterRequired)
        .NotEqual(model => model.OldPassword)
            .WithMessage(ValidationConstants.NewPasswordCannotBeTheSameAsOldOne);

        RuleFor(x => x.ConfirmNewPassword)
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
            .WithMessage(ValidationConstants.PasswordSpecialCharacterRequired)
        .Equal(model => model.NewPassword)
            .WithMessage(ValidationConstants.ConfirmPasswordDontMatchWithNewPassword);
    }
}