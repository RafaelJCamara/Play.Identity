using System;

namespace Play.Identity.Service.Exceptions
{
    [Serializable]
    public class InsufficientFundsException : Exception
    {
        public Guid UserId { get; }
        public decimal GilToDebit { get; }

        public InsufficientFundsException(Guid userId, decimal gilToDebit)
            : base($"Not enough gil to debit {gilToDebit} from user {userId}")
        {
            UserId = userId;
            GilToDebit = gilToDebit;
        }
    }
}
