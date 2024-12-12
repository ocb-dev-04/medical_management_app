using MassTransit;

namespace Services.Patients.Application.Consumers;

internal sealed class GetPatientCollectionByDoctorIdsConsumerDefinition
     : ConsumerDefinition<GetPatientCollectionByDoctorIdsConsumer>
{

    private readonly static string _consumerName = "get-patient-collection-by-doctor-id-queue";

    public GetPatientCollectionByDoctorIdsConsumerDefinition()
    {
        EndpointName = _consumerName;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<GetPatientCollectionByDoctorIdsConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.DiscardSkippedMessages();
        endpointConfigurator.ConfigureConsumeTopology = true;

        base.ConfigureConsumer(endpointConfigurator, consumerConfigurator, context);
    }
}
