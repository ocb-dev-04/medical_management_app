using MassTransit;

namespace Service.Diagnoses.Application.Consumers;

internal class GetDiagnosisCollectionByPatientIdConsumerDefinition
    : ConsumerDefinition<GetDiagnosisCollectionByPatientIdConsumer>
{
    private readonly static string _consumerName = "get-diagnosis-collection-by-patient-id-queue";

    public GetDiagnosisCollectionByPatientIdConsumerDefinition()
    {
        EndpointName = _consumerName;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<GetDiagnosisCollectionByPatientIdConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.DiscardSkippedMessages();
        endpointConfigurator.ConfigureConsumeTopology = true;

        base.ConfigureConsumer(endpointConfigurator, consumerConfigurator, context);
    }
}
