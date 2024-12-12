using Shared.Common.Helper.ErrorsHandler;

namespace Services.Patients.Domain.Errors;

public sealed class PatientErrors
{
    public static Error NotFound
        = Error.NotFound("patientNotFound", "The patient was not found");

    public static Error NotModified
        = Error.BadRequest("patientNotModified", "The patient was not modified");

    public static Error AlreadyExist
        = Error.BadRequest("patientAlreadyExist", "The patient already exist");

    public static Error YouAreNotTheOwner
        = Error.BadRequest("youAreNotTheOwner", "You not are the owner");

}
