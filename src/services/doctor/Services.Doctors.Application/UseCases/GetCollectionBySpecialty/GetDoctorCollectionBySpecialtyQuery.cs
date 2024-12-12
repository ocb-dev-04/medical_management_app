using FluentValidation;
using Shared.Domain.Constants;
using Services.Doctors.Domain.Dtos;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Doctors.Application.UseCases;

public sealed record GetDoctorCollectionBySpecialtyQuery(string Specialty, int PageNumber) 
    : IQuery<IEnumerable<DoctorDto>>;

internal sealed class GetDoctorCollectionBySpecialtyQueryValidator
    : AbstractValidator<GetDoctorCollectionBySpecialtyQuery>
{
    public GetDoctorCollectionBySpecialtyQueryValidator()
    {
        RuleFor(x => x.Specialty)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField)
        .MaximumLength(50)
            .WithMessage(ValidationConstants.LongField);

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