using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Play.Identity.Contracts;
using Play.Identity.Service.Entities;
using Play.Identity.Service.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;
using static Play.Identity.Service.Dtos.Dtos;

namespace Play.Identity.Service.Controllers
{
    [ApiController]
    [Route("Users")]
    [Authorize(Policy = LocalApi.PolicyName, Roles = Roles.Roles.Admin)]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IPublishEndpoint publishEndpoint;

        public UsersController(UserManager<ApplicationUser> userManager, IPublishEndpoint publishEndpoint)
        {
            this.userManager = userManager;
            this.publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserDto>> Get()
        {
            var users = userManager.Users
                                    .ToList()
                                    .Select(user => user.AsDto());
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetByIdAsync(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());

            if (user == null) return NotFound();

            return Ok(user.AsDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(Guid id, UpdateUserDto userDto)
        {
            var user = await userManager.FindByIdAsync(id.ToString());

            if (user == null) return NotFound();

            user.Email = userDto.Email;
            // the concention in ASP.NET Core Identity is that the username must be the same as the email
            user.UserName = userDto.Email;
            user.Gil = userDto.Gil;

            await userManager.UpdateAsync(user);

            await publishEndpoint.Publish(new UserUpdated(user.Id, user.Email, user.Gil));

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());

            if (user == null) return NotFound();

            await userManager.DeleteAsync(user);

            /*
                We could have two options here>
                  (1) Create a delete user contract
                  (2) Don't delete the user and make him have 0 gil, thus not being able to make any purchases in the platform
             */
            await publishEndpoint.Publish(new UserUpdated(user.Id, user.Email, 0));

            return NoContent();
        }

    }
}
