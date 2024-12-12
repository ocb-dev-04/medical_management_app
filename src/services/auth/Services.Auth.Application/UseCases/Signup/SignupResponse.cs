namespace Services.Auth.Application.UseCases;

public sealed record SignupResponse(
    string Token,
    CredentialResponse Credential);