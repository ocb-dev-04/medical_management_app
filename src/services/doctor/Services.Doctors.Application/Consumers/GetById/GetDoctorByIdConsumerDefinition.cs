using MassTransit;

namespace Services.Doctors.Application.Consumers;

internal sealed class GetDoctorByIdConsumerDefinition
     : ConsumerDefinition<GetDoctorByIdConsumer>
{
    private readonly static string _consumerName = "get-doctor-by-id-queue";

    public GetDoctorByIdConsumerDefinition()
    {
        EndpointName = _consumerName;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<GetDoctorByIdConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.DiscardSkippedMessages();
        endpointConfigurator.ConfigureConsumeTopology = true;

        base.ConfigureConsumer(endpointConfigurator, consumerConfigurator, context);
    }
}
