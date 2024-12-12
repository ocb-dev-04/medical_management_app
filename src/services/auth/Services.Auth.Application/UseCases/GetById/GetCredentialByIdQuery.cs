using FluentValidation;
using Shared.Domain.Constants;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Auth.Application.UseCases;

internal sealed record GetCredentialByIdQuery(Guid Id) 
    : IQuery<CredentialResponse>;

internal sealed class GetCredentialByIdQueryValidator
    : AbstractValidator<GetCredentialByIdQuery>
{
    public GetCredentialByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage(ValidationConstants.FieldCantBeEmpty)
        .NotNull()
            .WithMessage(ValidationConstants.RequiredField);
    }
}