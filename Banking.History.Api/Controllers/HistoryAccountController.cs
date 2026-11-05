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
        [HttpGet]
        public async Task<IActionResult> GetHistoryAccount([FromQuery]DtoAccount account)
        {

            var errors = AccountValidator.Validate(account);

            if (errors.Count > 0)
            {
                return BadRequest(new
                {
                    Message = "Invalid operation data.",
                    Errors = errors
                });
            }

            //await _accountMovement.MoveAccount(dto, token);


            return Ok(new
            {
                Message = "Operation validated successfully."
            });
        }
    }
}
