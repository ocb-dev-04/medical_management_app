using Common.Services.Hashing.Abstractions;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using Services.Auth.Domain.Abstractions;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.Errors;
using Services.Auth.Domain.StrongIds;
using Shared.Common.Helper.ErrorsHandler;
using Shared.Common.Helper.Models;
using Shared.Common.Helper.Providers;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Auth.Application.UseCases;

internal sealed class ChangePasswordCommandHandler
    : ICommandHandler<ChangePasswordCommand>
{
    private readonly ICredentialRepository _credentialRepository;
    private readonly IHashingService _hashService;

    private readonly HttpRequestProvider _httpRequestProvider;
    private readonly EntitiesEventsManagementProvider _entitiesEventsManagement;

    public ChangePasswordCommandHandler(
        IHashingService hashService,
        ICredentialRepository credentialRepository,

        HttpRequestProvider httpRequestProvider,
        EntitiesEventsManagementProvider entitiesEventsManagement)
    {
        ArgumentNullException.ThrowIfNull(hashService, nameof(hashService));
        ArgumentNullException.ThrowIfNull(credentialRepository, nameof(credentialRepository));

        ArgumentNullException.ThrowIfNull(httpRequestProvider, nameof(httpRequestProvider));
        ArgumentNullException.ThrowIfNull(entitiesEventsManagement, nameof(entitiesEventsManagement));

        _hashService = hashService;
        _credentialRepository = credentialRepository;

        _httpRequestProvider = httpRequestProvider;
        _entitiesEventsManagement = entitiesEventsManagement;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        Result<CurrentRequestUser> currentUser = _httpRequestProvider.GetContextCurrentUser();
        if (currentUser.IsFailure)
            return Result.Failure(currentUser.Error);

        Result<CredentialId> credentialIdResult = CredentialId.Create(currentUser.Value.CredentialId);
        if (credentialIdResult.IsFailure)
            return Result.Failure(credentialIdResult.Error);

        Result<Credential> found = await _credentialRepository.ByIdAsync(credentialIdResult.Value, cancellationToken);
        if (found.IsFailure)
            return Result.Failure(found.Error);

        if (!_hashService.Verify(request.OldPassword, found.Value.Password.Value))
            return Result.Failure(CredentialErrors.WrongPassword);

        string newPasswordHashed = _hashService.Hash(request.NewPassword);
        found.Value.UpdatePassword(StringObject.Create(newPasswordHashed));

        await _credentialRepository.CommitAsync(cancellationToken);

        return Result.Success();
    }
}