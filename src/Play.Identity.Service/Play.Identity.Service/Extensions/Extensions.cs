using Play.Identity.Service.Entities;
using static Play.Identity.Service.Dtos.Dtos;

namespace Play.Identity.Service.Extensions
{
    public static class Extensions
    {
        public static UserDto AsDto(this ApplicationUser user)
        {
            return new UserDto(user.Id, user.UserName, user.Email, user.Gil, user.CreatedOn);
        }
    }
}
