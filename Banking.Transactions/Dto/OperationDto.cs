using Banking.Barramento.Api.Enums;
using System.Security.Cryptography;

namespace Banking.Barramento.Api.Dto
{
    public class OperationDto
    {
        public Guid TransactionId { get; set; }
        public OperationType OperationType { get; set; }
        public decimal Value { get; set; }
        public string  AccountNumber{ get; set; } = string.Empty;
    }
}
