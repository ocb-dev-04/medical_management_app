using CQRS.MediatR.Helper.Abstractions.Messaging;
using FluentValidation;
using Shared.Domain.Constants;

namespace Services.Doctors.Application.UseCases;

public sealed record GetDoctorByIdQuery(Guid Id) 
    : IQuery<DoctorResponse>;

internal sealed class GetDoctorByIdQueryValidator
    : AbstractValidator<GetDoctorByIdQuery>
{
    public GetDoctorByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField);
    }
}