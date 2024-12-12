using Common.Services.Bus.Abstractions;
using MassTransit;
using MediatR;
using Service.Diagnoses.Application.UseCases;
using Shared.Common.Helper.ErrorsHandler;
using Shared.Common.Helper.Extensions;
using Shared.Common.Helper.Models.QueueResponses;
using Shared.Message.Queue.Requests.Requests;
using Shared.Message.Queue.Requests.Responses;

namespace Service.Diagnoses.Application.Consumers;

internal sealed class GetDiagnosisCollectionByPatientIdConsumer
    : IConsumer<GetDiagnosisCollectionByPatientIdRequest>
{
    #region Props & ctor

    private readonly ISender _sender;
    private readonly IExecuteHandlerService _executeHandlerRepository;

    public GetDiagnosisCollectionByPatientIdConsumer(
        ISender sender,
        IExecuteHandlerService executeHandlerRepository)
    {
        ArgumentNullException.ThrowIfNull(sender, nameof(sender));
        ArgumentNullException.ThrowIfNull(executeHandlerRepository, nameof(executeHandlerRepository));

        _sender = sender;
        _executeHandlerRepository = executeHandlerRepository;
    }

    #endregion

    public async Task Consume(ConsumeContext<GetDiagnosisCollectionByPatientIdRequest> context)
        => await _executeHandlerRepository.Execute(() => Process(context), context);

    private async Task Process(ConsumeContext<GetDiagnosisCollectionByPatientIdRequest> context)
    {
        GetDiagnosisCollectionByPatientIdQuery query = new(context.Message.Id, context.Message.PageNumber);
        Result<IEnumerable<DiagnosisResponse>> queryResponse = await _sender.Send(query, context.CancellationToken);

        IEnumerable<DiagnosisQueueResponse> collection = queryResponse.Value.Select(s => s.MapToQueueResponse());
        string serialize = new DiagnosisCollectionQueueResponse(collection).Serialize();

        BusMessageResponse response = new BusMessageResponse().Done(serialize);
        BusMessageResult result = new(response.Serialize());
        await context.RespondAsync<BusMessageResult>(result);
    }
}