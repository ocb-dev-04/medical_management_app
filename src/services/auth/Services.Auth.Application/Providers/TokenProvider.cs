using System.Text;
using System.Security.Claims;

using Shared.Common.Helper.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Shared.Common.Helper.ErrorsHandler;
using Services.Auth.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Services.Auth.Application.Settings;

namespace Services.Auth.Application.Providers;

internal sealed class TokenProvider
{
    private static readonly Error _authoritationHeaderNotFound
        = Error.NotFound("authoritationHeaderNotFound", "The jwt was not found");

    private static readonly string _headerAuthKey = "Authorization";
    private static readonly string _securityAlgorithm = SecurityAlgorithms.HmacSha256Signature;

    internal Result<string> BuildJwt(
        in Credential model,
        in JwtSettings jwtSettings,
        in JwtSecurityTokenHandler jwtSecurityTokenHandler)
    {
        HashSet<Claim> claims = new()
                {
                    new Claim(ClaimTypes.Email, model.Email.Value),
                    new Claim(ClaimTypes.Sid, model.Id.Value.ToString()),
                    new Claim(ClaimTypes.Hash, model.PrivateKey.Value),
                };

#if DEBUG
        DateTime expirationToken = DateTime.UtcNow.AddDays(1);
#else
        DateTime expirationToken = DateTime.UtcNow.AddMinutes(30);
#endif
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(jwtSettings.IssuerSigningKey));
        SigningCredentials credentials = new(securityKey, _securityAlgorithm);
        JwtSecurityToken tokenDescriptor = new(
                issuer: jwtSettings.ValidIssuer,
                audience: jwtSettings.ValidAudience,
                claims: claims,
                expires: expirationToken,
                signingCredentials: credentials
            );

        return jwtSecurityTokenHandler.WriteToken(tokenDescriptor);
    }

    internal Result<Guid> ReadJwt(
        in JwtSettings jwtSettings,
        in JwtSecurityTokenHandler jwtSecurityTokenHandler,
        in HttpRequestProvider httpRequestProvider)
    {
        HttpContext httpContext = httpRequestProvider.GetCurrentHttpContext()!;
        bool getToken = httpContext.Request.Headers.TryGetValue(_headerAuthKey, out StringValues jwt);
        if (!getToken)
            return Result.Failure<Guid>(_authoritationHeaderNotFound);

        string token = jwt.ToString().Substring("Bearer ".Length).Trim();
        if (!jwtSecurityTokenHandler.CanReadToken(token))
            return Result.Failure<Guid>(Error.Unauthorized());

        JwtSecurityToken jwtToken = jwtSecurityTokenHandler.ReadJwtToken(token);

        TokenValidationParameters validationParameters = ValidationParameters(jwtSettings);
        ClaimsPrincipal principal = jwtSecurityTokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
        if (validatedToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(_securityAlgorithm, StringComparison.InvariantCultureIgnoreCase))
            return Result.Failure<Guid>(Error.Unauthorized());

        IEnumerable<Claim> claims = principal.Claims;
        string email = claims.FirstOrDefault((Claim s) => s.Type.Equals(ClaimTypes.Email))!.Value;
        string credentialId = claims.FirstOrDefault((Claim s) => s.Type.Equals(ClaimTypes.Sid))!.Value;
        string privateKey = claims.FirstOrDefault((Claim s) => s.Type.Equals(ClaimTypes.Hash))!.Value;

        return Guid.Parse(credentialId);
    }

    private static TokenValidationParameters ValidationParameters(JwtSettings jwtSettings)
        => new TokenValidationParameters
        {
            ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.IssuerSigningKey)),

            RequireExpirationTime = false,
            ValidateLifetime = false,

            ValidateIssuer = jwtSettings.ValidateIssuer,
            ValidIssuer = jwtSettings.ValidIssuer,

            ValidateAudience = jwtSettings.ValidateAudience,
            ValidAudience = jwtSettings.ValidAudience
        };
}
