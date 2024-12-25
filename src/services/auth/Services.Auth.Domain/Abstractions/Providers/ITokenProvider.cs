using Services.Auth.Domain.Entities;
using Services.Auth.Domain.Settings;
using Shared.Common.Helper.Providers;
using System.IdentityModel.Tokens.Jwt;
using Shared.Common.Helper.ErrorsHandler;

namespace Services.Auth.Domain.Abstractions.Providers;

public interface ITokenProvider
{
    /// <summary>
    /// Build a token
    /// </summary>
    /// <param name="model"></param>
    /// <param name="jwtSettings"></param>
    /// <param name="jwtSecurityTokenHandler"></param>
    /// <returns></returns>
    public string BuildJwt(
        in Credential model,
        in JwtSettings jwtSettings,
        in JwtSecurityTokenHandler jwtSecurityTokenHandler);

    /// <summary>
    /// Read a toen
    /// </summary>
    /// <param name="jwtSettings"></param>
    /// <param name="jwtSecurityTokenHandler"></param>
    /// <param name="httpRequestProvider"></param>
    /// <returns></returns>
    public Result<Guid> ReadJwt(
        in JwtSettings jwtSettings,
        in JwtSecurityTokenHandler jwtSecurityTokenHandler,
        in HttpRequestProvider httpRequestProvider);
}
