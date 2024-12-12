namespace Services.Auth.Application.UseCases;

public sealed record SigninResponse(
    string Token,
    CredentialResponse Credential);