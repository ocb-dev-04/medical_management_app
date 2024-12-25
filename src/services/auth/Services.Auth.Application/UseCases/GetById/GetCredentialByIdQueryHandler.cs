using Services.Auth.Domain.Errors;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.StrongIds;
using Shared.Common.Helper.ErrorsHandler;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using Services.Auth.Domain.Abstractions.Repositories;

namespace Services.Auth.Application.UseCases;

internal sealed class GetCredentialByIdQueryHandler
    : IQueryHandler<GetCredentialByIdQuery, CredentialResponse>
{
    private readonly ICredentialRepository _credentialRepository;

    public GetCredentialByIdQueryHandler(ICredentialRepository credentialRepository)
    {
        ArgumentNullException.ThrowIfNull(credentialRepository, nameof(credentialRepository));

        _credentialRepository = credentialRepository;
    }

    public async Task<Result<CredentialResponse>> Handle(GetCredentialByIdQuery request, CancellationToken cancellationToken)
    {
        Result<CredentialId> credentialId = CredentialId.Create(request.Id);
        if (credentialId.IsFailure)
            return Result.Failure<CredentialResponse>(CredentialErrors.NotFound);

        Result<Credential> found = await _credentialRepository.ByIdAsync(credentialId.Value, cancellationToken);
        if(found.IsFailure)
            return Result.Failure<CredentialResponse>(found.Error);

        return CredentialResponse.Map(found.Value);
    }
}
