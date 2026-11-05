using Banking.Barramento.Api.Application;
using Banking.Barramento.Api.Application.Interface;
using Banking.Barramento.Api.Dto;
using Banking.Barramento.Api.Validator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Barramento.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionAccountController : ControllerBase
    {
        private readonly IAccountMovement _accountMovement;

        private readonly ILogger<TransactionAccountController> _logger;
        public TransactionAccountController(IAccountMovement accountMovement,
                                            ILogger<TransactionAccountController> logger)
        {
            _accountMovement = accountMovement;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> TransactionOperation(OperationDto dto,CancellationToken token)
        {
            var errors = OperationValidator.Validate(dto);

            if (errors.Count > 0)
            {
                return BadRequest(new
                {
                    Message = "Invalid operation data.",
                    Errors = errors
                });
            }
 
            await _accountMovement.MoveAccount(dto,token);

            return Ok(new
            {
                Message = "Operation validated successfully."
            });
        }
    }
}
