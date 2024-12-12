using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Services.Auth.Application.UseCases;

public sealed record RefreshTokenQuery() 
    : IQuery<RefreshTokenResponse>;