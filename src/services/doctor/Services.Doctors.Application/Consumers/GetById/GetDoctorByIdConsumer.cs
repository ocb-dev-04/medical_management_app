using MediatR;
using MassTransit;
using Shared.Common.Helper.Extensions;
using Common.Services.Bus.Abstractions;
using Shared.Common.Helper.ErrorsHandler;
using Services.Doctors.Application.UseCases;
using Shared.Message.Queue.Requests.Requests;
using Shared.Message.Queue.Requests.Responses;
using Shared.Common.Helper.Models.QueueResponses;

namespace Services.Doctors.Application.Consumers;

internal sealed class GetDoctorByIdConsumer
    : IConsumer<GetDoctorByIdRequest>
{
    #region Props & ctor

    private readonly ISender _sender;
    private readonly IExecuteHandlerService _executeHandlerRepository;

    public GetDoctorByIdConsumer(
        ISender sender,
        IExecuteHandlerService executeHandlerRepository)
    {
        ArgumentNullException.ThrowIfNull(sender, nameof(sender));
        ArgumentNullException.ThrowIfNull(executeHandlerRepository, nameof(executeHandlerRepository));

        _sender = sender;
        _executeHandlerRepository = executeHandlerRepository;
    }

    #endregion
    public async Task Consume(ConsumeContext<GetDoctorByIdRequest> context)
        => await _executeHandlerRepository.Execute(() => Process(context), context);

    private async Task Process(ConsumeContext<GetDoctorByIdRequest> context)
    {
        GetDoctorByIdQuery query = new(context.Message.Id);
        Result<DoctorResponse> queryResponse = await _sender.Send(query, context.CancellationToken);

        BusMessageResponse response = queryResponse.IsSuccess
            ? new BusMessageResponse().Done(
                DoctorQueueResponse.Map(
                    queryResponse.Value.Id, 
                    queryResponse.Value.Name, 
                    queryResponse.Value.Specialty, 
                    queryResponse.Value.ExperienceInYears, 
                    queryResponse.Value.CreatedOnUtc).Serialize())
            : new BusMessageResponse().NotFound();

        BusMessageResult result = new(response.Serialize());
        await context.RespondAsync<BusMessageResult>(result);
    }
}