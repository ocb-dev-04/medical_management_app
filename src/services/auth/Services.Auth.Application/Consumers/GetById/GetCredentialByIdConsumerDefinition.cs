using MassTransit;

namespace Services.Auth.Application.Consumers;

internal sealed class GetCredentialByIdConsumerDefinition
    : ConsumerDefinition<GetCredentialByIdConsumer>
{
    private readonly static string _consumerName = "get-credential-by-id-queue";

    public GetCredentialByIdConsumerDefinition()
    {
        EndpointName = _consumerName;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<GetCredentialByIdConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.DiscardSkippedMessages();
        endpointConfigurator.ConfigureConsumeTopology = true;

        base.ConfigureConsumer(endpointConfigurator, consumerConfigurator, context);
    }
}
