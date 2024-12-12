using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Shared.Common.Helper.Extensions;
using Shared.Common.Helper.ErrorsHandler;
using System.ComponentModel.DataAnnotations;
using Service.Diagnoses.Application.UseCases;
using Service.Diagnoses.Presentation.Controllers.Base;

namespace Service.Diagnoses.Presentation.Controllers;

[ApiController]
[Route("diagnoses")]
[Produces("application/json")]
public sealed class DiagnosesController : BaseController
{
    public DiagnosesController(ISender sender) : base(sender)
    {
    }

    #region Queries

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DiagnosisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromRoute, Required] Guid id, CancellationToken cancellationToken)
    {
        GetDiagnosisByIdQuery query = new(id);
        Result<DiagnosisResponse> response = await _sender.Send(query, cancellationToken);

        return response.Match(Ok, HandleErrorResults);
    }

    [HttpGet("by-patient")]
    [ProducesResponseType(typeof(IEnumerable<DiagnosisResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCollectionByPatientId(
        [FromQuery, Required] Guid patientid,
        [FromQuery, Required] int pageNumber,
        CancellationToken cancellationToken)
    {
        GetDiagnosisCollectionByPatientIdQuery query = new(patientid, pageNumber);
        Result<IEnumerable<DiagnosisResponse>> response = await _sender.Send(query, cancellationToken);

        return response.Match(Ok, HandleErrorResults);
    }
    
    [HttpGet("dosage-intervals")]
    [ProducesResponseType(typeof(IEnumerable<DosageIntervalResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDosageIntervalCollection(CancellationToken cancellationToken)
    {
        GetDosageIntervalCollectionQuery query = new();
        Result<IEnumerable<DosageIntervalResponse>> response = await _sender.Send(query, cancellationToken);

        return response.Match(Ok, HandleErrorResults);
    }

    #endregion

    #region Commands

    [HttpPost]
    [ProducesResponseType(typeof(DiagnosisResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDiagnosisCommand command,
        CancellationToken cancellationToken)
    {
        Result<DiagnosisResponse> response = await _sender.Send(command, cancellationToken);

        return response.Match(
                success: data => Created(string.Empty, data),
                error: HandleErrorResults);
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
        RemoveDiagnosisCommand command = new(id, doctorId);
        Result response = await _sender.Send(command, cancellationToken);

        return response.Match(Ok, HandleErrorResults);
    }

    #endregion
}
