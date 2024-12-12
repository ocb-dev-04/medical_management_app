using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

using Shared.Common.Helper.Extensions;
using Services.Auth.Application.UseCases;
using Shared.Common.Helper.ErrorsHandler;
using Services.Auth.Presentation.Controllers.Base;

namespace Services.Auth.Presentation.Controllers;

[ApiController]
[Route("auth")]
[Produces("application/json")]
public sealed class AuthController : BaseController
{
    public AuthController(
        ISender sender) : base(sender)
    {
    }

    #region Queries

    /// <summary>
    /// Refresh jwt token
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("refresh-token")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        RefreshTokenQuery query = new();
        Result<RefreshTokenResponse> response = await _sender.Send(query, cancellationToken);

        return response.Match(Ok, HandleErrorResults);
    }

    /// <summary>
    /// Check access and return credential related to token
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("check-access")]
    [ProducesResponseType(typeof(CredentialResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckAccess(CancellationToken cancellationToken)
    {
        GetCredentialByTokenQuery query = new();
        Result<CredentialResponse> response = await _sender.Send(query, cancellationToken);

        return response.Match(value => Ok(value), HandleErrorResults);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Signup with credentials
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("sign-up")]
    [ProducesResponseType(typeof(SignupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Signup([FromBody] SignupCommand command, CancellationToken cancellationToken)
    {
        Result<SignupResponse> response = await _sender.Send(command, cancellationToken);

        return response.Match(
                success: data => Created(string.Empty, data),
                error: HandleErrorResults);
    }

    /// <summary>
    /// Login with credentials
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("sign-in")]
    [ProducesResponseType(typeof(SigninResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SignIn([FromBody] SigninCommand command, CancellationToken cancellationToken)
    {
        Result<SigninResponse> response = await _sender.Send(command, cancellationToken);

        return response.Match(Ok, HandleErrorResults);
    }

    /// <summary>
    /// Change credentials password
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        Result response = await _sender.Send(command, cancellationToken);

        return response.Match(Ok, HandleErrorResults);
    }

    #endregion
}
