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
        public async Task<IActionResult> GetHistoryAccount([FromQuery] DtoAccount request, CancellationToken token)
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

            var accountStatements = await _historyMovement.GetHistoryAsync(request, token);

            return Ok(accountStatements);
        }
    }
}
