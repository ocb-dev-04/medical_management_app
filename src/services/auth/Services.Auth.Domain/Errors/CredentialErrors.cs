using Shared.Common.Helper.ErrorsHandler;

namespace Services.Auth.Domain.Errors;

public sealed class CredentialErrors
{
    public static Error NotFound
        = Error.NotFound("credentialNotFound", "The credential was not found");

    public static Error NotModified
        = Error.BadRequest("credentialNotModified", "The credential was not modified");


    public static Error EmailNotFound
        = Error.NotFound("emailNotFound", "The email not found");

    public static Error EmailAlreadyExist
        = Error.BadRequest("emailAlreadyExist", "The email already exist");


    public static Error WrongPassword
        = Error.BadRequest("wrongPassword", "The password don't match");

    public static Error PasswordNoChanges
        = Error.BadRequest("passwordNoChanges", "The password is the same");
}
