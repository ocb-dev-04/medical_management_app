using MediatR;
using MassTransit;
using Shared.Message.Queue.Requests;
using Shared.Common.Helper.Extensions;
using Common.Services.Bus.Abstractions;
using Services.Auth.Application.UseCases;
using Shared.Common.Helper.ErrorsHandler;
using Shared.Message.Queue.Requests.Requests;
using Shared.Common.Helper.Models.QueueResponses;

namespace Services.Auth.Application.Consumers;

internal sealed class GetCredentialByIdConsumer
    : IConsumer<GetCredentialByIdRequest>
{
    #region Props & ctor

    private readonly ISender _sender;
    private readonly IExecuteHandlerService _executeHandlerRepository;

    public GetCredentialByIdConsumer(
        ISender sender,
        IExecuteHandlerService executeHandlerRepository)
    {
        ArgumentNullException.ThrowIfNull(sender, nameof(sender));
        ArgumentNullException.ThrowIfNull(executeHandlerRepository, nameof(executeHandlerRepository));

        _sender = sender;
        _executeHandlerRepository = executeHandlerRepository;
    }

    #endregion
    public async Task Consume(ConsumeContext<GetCredentialByIdRequest> context)
        => await _executeHandlerRepository.Execute(() => Process(context), context);

    private async Task Process(ConsumeContext<GetCredentialByIdRequest> context)
    {
        GetCredentialByIdQuery query = new(context.Message.Id);
        Result<CredentialResponse> queryResponse = await _sender.Send(query, context.CancellationToken);

        BusMessageResponse response = queryResponse.IsSuccess
            ? new BusMessageResponse().Done(
                CredentialQueueResponse.Map(
                    queryResponse.Value.Id, 
                    queryResponse.Value.Email, 
                    queryResponse.Value.CreatedOnUtc, 
                    queryResponse.Value.ModifiedOnUtc).Serialize())
            : new BusMessageResponse().NotFound();

        BusMessageResult result = new(response.Serialize());
        await context.RespondAsync<BusMessageResult>(result);
    }
}
