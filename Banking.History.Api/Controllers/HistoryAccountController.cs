using Banking.History.Api.Application.Interface;
using Banking.History.Api.Dto;
using Banking.History.Api.Validator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Banking.History.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HistoryAccountController : ControllerBase
    {
        private readonly IHistoryMovement _historyMovement;
        public HistoryAccountController(IHistoryMovement historyMovement)
        {
            _historyMovement = historyMovement;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistoryAccount([FromQuery] DtoAccount request, [FromQuery] int page = 1,[FromQuery] int pageSize = 20,CancellationToken token=default)
        {
            var errors = AccountValidator.Validate(request);

            if (errors.Count > 0)
            {
                return BadRequest(new
                {
                    Message = "Invalid operation data.",
                    Errors = errors
                });
            }

            if (page < 1)
            {
                return BadRequest(new
                {
                    Message = "Page must be greater than zero."
                });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new
                {
                    Message = "PageSize must be between 1 and 100."
                });
            }

            var accountStatement =
                await _historyMovement.GetHistoryAsync(
                    request,
                    page,
                    pageSize,
                    token);

            if (accountStatement is null)
            {
                return NotFound(new
                {
                    Message = "Account not found."
                });
            }

            return Ok(accountStatement);
        }
    }
}
