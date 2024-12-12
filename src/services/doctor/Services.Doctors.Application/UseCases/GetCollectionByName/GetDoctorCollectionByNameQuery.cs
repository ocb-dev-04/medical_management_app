using FluentValidation;
using Shared.Domain.Constants;
using Services.Doctors.Domain.Dtos;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Doctors.Application.UseCases;

public sealed record GetDoctorCollectionByNameQuery(string Name, int PageNumber) 
    : IQuery<IEnumerable<DoctorDto>>;

internal sealed class GetDoctorCollectionByNameQueryValidator
    : AbstractValidator<GetDoctorCollectionByNameQuery>
{
    public GetDoctorCollectionByNameQueryValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField)
        .MaximumLength(100)
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