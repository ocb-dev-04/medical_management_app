using Service.Diagnoses.Application.UseCases;

namespace Services.Diagnoses.Application.UnitTests.UseCases.Remove;

public sealed class RemoveDiagnosisCommandHandlerTest
    : BaseTestSharedConfiguration
{
    private readonly RemoveDiagnosisCommand _command;
    private readonly RemoveDiagnosisCommandHandler _handler;

    public RemoveDiagnosisCommandHandlerTest()
    {
        _command = new(_validPatient.Id, _validDoctor.Id);
        _handler = new(
            _diagnosisRepositoryMock, 
            _messageQeueServicesMock);
    }
}
