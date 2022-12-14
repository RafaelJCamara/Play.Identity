using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Play.Identity.Service.Entities;
using Play.Identity.Service.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace Play.Identity.Service.HostedServices
{
    public class IdentitySeedHostedService : IHostedService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IdentitySettings settings;

        public IdentitySeedHostedService(
            IServiceScopeFactory serviceScopeFactory,
            IOptions<IdentitySettings> identityOptions)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            settings = identityOptions.Value;
        }

        /*
            runs when the service starts
            what we want to do here is to create the player role and the admin role
            also, we want to create the admin user
            with this, our service starts already with those two roles created and the admin user created
         */
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceScopeFactory.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            //here we are going to effectively create the two main roles
            await CreateRoleIfNotExistsAsync(Roles.Roles.Admin, roleManager);
            await CreateRoleIfNotExistsAsync(Roles.Roles.Player, roleManager);

            var adminUser = await userManager.FindByEmailAsync(settings.AdminUserEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = settings.AdminUserEmail,
                    Email = settings.AdminUserEmail
                };

                //here we are creating the admin users and also assigning the admin role to him
                await userManager.CreateAsync(adminUser, settings.AdminUserPassword);
                await userManager.AddToRoleAsync(adminUser, Roles.Roles.Admin);
            }
        }

        /*
            runs when the services ends
            in our case, we don't need to do nothing when the service ends
         */
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static async Task CreateRoleIfNotExistsAsync(
            string role,
            RoleManager<ApplicationRole> roleManager
        )
        {
            var roleExists = await roleManager.RoleExistsAsync(role);

            if (!roleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
        }
    }
}
