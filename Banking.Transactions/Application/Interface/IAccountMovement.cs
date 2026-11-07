using Banking.Barramento.Api.Dto;

namespace Banking.Barramento.Api.Application.Interface
{
    public interface IAccountMovement
    {
        Task MoveAccount(OperationDto operation, CancellationToken cancellation);
    }
}
