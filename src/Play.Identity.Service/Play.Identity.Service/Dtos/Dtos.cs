using System;
using System.ComponentModel.DataAnnotations;

namespace Play.Identity.Service.Dtos
{
    public class Dtos
    {
        public record UserDto(Guid id, string username, string Email, decimal Gil, DateTimeOffset CreatedDate);
        public record UpdateUserDto([Required] [EmailAddress] string Email, [Range(0, 1000000)] decimal Gil);
    }
}
