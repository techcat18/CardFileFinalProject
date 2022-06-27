﻿using BLL.Abstractions.cs.Interfaces;
using BLL.Services;
using BLL.Validation;
using CardFileApi.Logging;
using Core.DTOs;
using Core.Models;
using Core.RequestFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CardFileApi.Controllers
{
    /// <summary>
    /// Controller that provides endpoints for working with text materials
    /// </summary>
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Route("api/textMaterials")]
    public class TextMaterialController : ControllerBase
    {
        private readonly ITextMaterialService _textMaterialService;
        private readonly ILoggerManager _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Constructor that takes three arguments
        /// </summary>
        /// <param name="textMaterialService">Instance of class that implements ITextMaterialService interface to work with text materials</param>
        /// <param name="logger">Instance of class that implements ILoggerManager interface to log information</param>
        /// <param name="httpContextAccessor">Instance of class that implements IHhttpContextAccessor to fetch the current user info</param>
        public TextMaterialController(ITextMaterialService textMaterialService,
            ILoggerManager logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _textMaterialService = textMaterialService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Returns all existing text materials from the database by given parameters
        /// </summary>
        /// <param name="parameters">Parameters that will be taken into account when return text materials</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<TextMaterialDTO>>> Get([FromQuery]TextMaterialParameters parameters)
        {
            var textMaterials = await _textMaterialService.GetTextMaterials(parameters);

            if (textMaterials == null)
            {
                _logger.LogInfo("No text materials were found");
                return NotFound("No text materials were found");
            }

            var metadata = new
            {
                textMaterials.TotalCount,
                textMaterials.PageSize,
                textMaterials.CurrentPage,
                textMaterials.TotalPages,
                textMaterials.HasNext,
                textMaterials.HasPrevious
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
            Response.Headers.Add("Access-Control-Expose-Headers", "X-Pagination");

            return Ok(textMaterials);
        }

        /// <summary>
        /// Returns single text material by its id
        /// </summary>
        /// <param name="id">Id of the text material to return</param>
        [HttpGet("{id}", Name = "GetTextMaterialById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TextMaterialDTO>> GetById(int id)
        {
            var textMaterial = await _textMaterialService.GetTextMaterialById(id);

            if (textMaterial == null)
            {
                _logger.LogInfo($"Text material with id {id} does not exist");
                return NotFound($"Text material with id {id} does not exist");
            }

            return Ok(textMaterial);
        }

        /// <summary>
        /// Adds new text material to the database
        /// </summary>
        /// <param name="textMaterialDTO">Data transfer object that contains info of the new text material</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody]CreateTextMaterialDTO textMaterialDTO)
        {
            try
            {
                var textMaterial = await _textMaterialService.CreateTextMaterial(textMaterialDTO);

                return CreatedAtRoute("GetTextMaterialById", new { id = textMaterial.Id }, textMaterial);
            }
            catch (CardFileException e)
            {
                _logger.LogInfo($"Failed to create a text material: {e.Message}");
                return BadRequest(e.Message);
            }
        }
        
        /// <summary>
        /// Sends on user's email text material's content and optionally its data by its id as a pdf file, available only for authenticated users
        /// </summary>
        /// <param name="id">Id of the text material to be sent on user's email as a pdf file</param>
        /// <param name="emailParams">Parameters that will be taken into account when generating a pdf file</param>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("{id}/print")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendAsPdf(int id,[FromQuery] EmailParameters emailParams)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                await _textMaterialService.SendTextMaterialAsPdf(userId, id, emailParams);

                return Ok();
            }
            catch (CardFileException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Approves the text material by its id, available only for users who are in Manager role
        /// </summary>
        /// <param name="id">Id of the text material to approve</param>
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Manager")]
        [HttpPut("{id}/approve")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                await _textMaterialService.ApproveTextMaterial(id);

                return NoContent();
            }
            catch (CardFileException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Rejects the text material by its id, available only for users who are in Manager role
        /// </summary>
        /// <param name="id">Id of the text material to reject</param>
        /// <param name="rejectMessage">Reason why the text material is rejected</param>
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Manager")]
        [HttpPut("{id}/reject")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Reject(int id,[FromBody] RejectMessageDTO rejectMessage)
        {
            try
            {
                await _textMaterialService.RejectTextMaterial(id, rejectMessage.RejectMessage);

                return NoContent();
            }
            catch (CardFileException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Updates existing text material in the database, available only for authenticated users
        /// </summary>
        /// <param name="textMaterialDTO">Data transfer object which contains data of the text material to update</param>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put([FromBody]UpdateTextMaterialDTO textMaterialDTO)
        {
            try
            {
                await _textMaterialService.UpdateTextMaterial(textMaterialDTO);
                
                return NoContent();
            }
            catch (CardFileException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Deleted the text material from the database by its id, available only for authenticated users
        /// </summary>
        /// <param name="id">Id of the text material to delete</param>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _textMaterialService.DeleteTextMaterial(id);

                return NoContent();
            }
            catch (CardFileException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
