using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Shared.Common.Helper.Extensions;
using Shared.Common.Helper.ErrorsHandler;
using System.ComponentModel.DataAnnotations;
using Services.Patients.Application.UseCases;
using Services.Patients.Presentation.Controllers.Base;

namespace Services.Patients.Presentation.Controllers;

[ApiController]
[Route("patients")]
[Produces("application/json")]
public sealed class PatientsController : BaseController
{
    public PatientsController(ISender sender) : base(sender)
    {
    }

    #region Queries

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromRoute, Required] Guid id, CancellationToken cancellationToken)
    {
        GetPatientByIdQuery query = new(id);
        Result<PatientResponse> response = await _sender.Send(query, cancellationToken);

        return response.Match(Ok, HandleErrorResults);
    }

    [HttpGet("by-doctor")]
    [ProducesResponseType(typeof(IEnumerable<PatientResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByPatientId(
        [FromQuery, Required] Guid doctorId,
        [FromQuery, Required] int pageNumber,
        CancellationToken cancellationToken)
    {
        GetPatientCollectionByDoctorIdQuery query = new(doctorId, pageNumber);
        Result<IEnumerable<PatientResponse>> response = await _sender.Send(query, cancellationToken);

        return response.Match(Ok, HandleErrorResults);
    }

    #endregion

    #region Commands

    [HttpPost]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePatientCommand command,
        CancellationToken cancellationToken)
    {
        Result<PatientResponse> response = await _sender.Send(command, cancellationToken);

        return response.Match(
                success: data => Created(string.Empty, data),
                error: HandleErrorResults);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        [FromRoute, Required] Guid id,
        [FromBody] UpdatePatientRequest request,
        CancellationToken cancellationToken)
    {
        UpdatePatientCommand command = new(id, request);
        Result<PatientResponse> response = await _sender.Send(command, cancellationToken);

        return response.Match(Ok, HandleErrorResults);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Remove(
        [FromRoute, Required] Guid id,
        [FromQuery, Required] Guid doctorId,
        CancellationToken cancellationToken)
    {
        RemovePatientCommand command = new(id, doctorId);
        Result response = await _sender.Send(command, cancellationToken);

        return response.Match(Ok, HandleErrorResults);
    }

    #endregion
}
