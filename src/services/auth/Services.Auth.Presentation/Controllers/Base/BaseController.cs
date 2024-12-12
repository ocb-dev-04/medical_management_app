using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Helper.ErrorsHandler;

namespace Services.Auth.Presentation.Controllers.Base;

public class BaseController : ControllerBase
{
    protected readonly ISender _sender;

    protected BaseController(
        ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender, nameof(sender));

        _sender = sender;
    }

    protected IActionResult HandleErrorResults(Error error)
        => error.StatusCode switch
        {
            304 => StatusCode(error.StatusCode, new { error.Translation, error.Description }),
            400 => BadRequest(new { error.Translation, error.Description }),
            401 => Unauthorized(),
            404 => NotFound(new { error.Translation, error.Description }),
            _ => StatusCode(error.StatusCode)
        };
}
