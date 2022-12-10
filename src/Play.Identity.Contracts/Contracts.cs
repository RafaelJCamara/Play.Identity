using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Play.Identity.Contracts
{
    public record DebitGil(Guid UserId, decimal Gil, Guid CorrelationId);
    public record GilDebited(Guid CorrelationId);
    public record UserUpdated(Guid UserId, string Email, decimal NewTotalGil);
}
