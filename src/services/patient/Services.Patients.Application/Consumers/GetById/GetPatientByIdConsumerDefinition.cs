using MassTransit;

namespace Services.Patients.Application.Consumers;

internal sealed class GetPatientByIdConsumerDefinition
    : ConsumerDefinition<GetPatientByIdConsumer>
{

    private readonly static string _consumerName = "get-patient-by-id-queue";

    public GetPatientByIdConsumerDefinition()
    {
        EndpointName = _consumerName;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<GetPatientByIdConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.DiscardSkippedMessages();
        endpointConfigurator.ConfigureConsumeTopology = true;

        base.ConfigureConsumer(endpointConfigurator, consumerConfigurator, context);
    }
}
