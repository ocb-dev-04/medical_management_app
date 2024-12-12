using Shared.Common.Helper.ErrorsHandler;

namespace Services.Doctors.Domain.Errors;

public sealed class DoctorErrors
{
    public static Error NotFound
        = Error.NotFound("doctorNotFound", "The doctor was not found");

    public static Error NotModified
        = Error.BadRequest("doctorNotModified", "The doctor was not modified");

    public static Error AlreadyExist
        = Error.BadRequest("doctorAlreadyExist", "The doctor already exist");

    public static Error YouAreNotTheOwner
        = Error.BadRequest("youAreNotTheOwner", "You not are the owner");

}
