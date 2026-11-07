using Banking.History.Api.Dto;

namespace Banking.History.Api.Validator
{
    public static class AccountValidator
    {
        public static List<string> Validate(DtoAccount account)
        {
            var errors = new List<string>();


            if(string.IsNullOrEmpty(account.Account))
            {
                errors.Add("Account number is required.");
            }



            return errors;
        }
    }
}
