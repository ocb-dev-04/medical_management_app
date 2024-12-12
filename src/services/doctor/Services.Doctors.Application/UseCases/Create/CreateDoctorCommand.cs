using FluentValidation;
using Shared.Domain.Constants;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Doctors.Application.UseCases;

public sealed record CreateDoctorRequest(
    string Name,
    string Specialty,
    int ExperienceInYears);

public sealed record CreateDoctorCommand(
    Guid CredentialId,
    CreateDoctorRequest Body) : ICommand<DoctorResponse>;

internal sealed class CreateDoctorCommandValidator
    : AbstractValidator<CreateDoctorCommand>
{
    private readonly static int _maxExperience = 60;
    private const string _experienceCantBeGreaterThanSixty = "experienceCantBeGreaterThanSixty";

    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.CredentialId)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField);

        RuleFor(x => x.Body.Name)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField)
        .MaximumLength(100)
            .WithMessage(ValidationConstants.LongField);

        RuleFor(x => x.Body.Specialty)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField)
        .MaximumLength(50)
            .WithMessage(ValidationConstants.LongField);

        RuleFor(x => x.Body.ExperienceInYears)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField)
        .LessThan(_maxExperience)
            .WithMessage(_experienceCantBeGreaterThanSixty);
    }
}