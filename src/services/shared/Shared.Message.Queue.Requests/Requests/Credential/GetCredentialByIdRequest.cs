using MassTransit;

namespace Shared.Message.Queue.Requests.Requests;

[EntityName("get-credential-by-id-bind")]
public sealed class GetCredentialByIdRequest
{
    public Guid Id { get; set; }

    public GetCredentialByIdRequest()
    {

    }

    public static GetCredentialByIdRequest Create(Guid id)
        => new GetCredentialByIdRequest { Id = id };
}