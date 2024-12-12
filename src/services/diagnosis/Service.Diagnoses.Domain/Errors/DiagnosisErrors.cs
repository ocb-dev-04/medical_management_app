using Shared.Common.Helper.ErrorsHandler;

namespace Service.Diagnoses.Domain.Errors;

public sealed class DiagnosisErrors
{
    public static Error NotFound
        = Error.NotFound("diagnosisNotFound", "The diagnosis was not found");

    public static Error NotModified
        = Error.BadRequest("diagnosisNotModified", "The diagnosis was not modified");

    public static Error AlreadyExist
        = Error.BadRequest("diagnosisAlreadyExist", "The diagnosis already exist");

    public static Error YouAreNotTheOwner
        = Error.BadRequest("youAreNotTheOwner", "You not are the owner");
}
