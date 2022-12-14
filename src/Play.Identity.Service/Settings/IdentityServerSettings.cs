using IdentityServer4.Models;
using System;
using System.Collections.Generic;

namespace Play.Identity.Service.Settings
{
    public class IdentityServerSettings
    {
        public IReadOnlyCollection<ApiScope> ApiScopes { get; init; }
        public IReadOnlyCollection<ApiResource> ApiResources { get; init; }
        public IReadOnlyCollection<Client> Clients { get; init; }
        public IReadOnlyCollection<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                //this means that whenever a user requests an identity token, we are saying that it should also return the roles claim
                new IdentityResource("roles", new[] {"role"})
            };
    }
}
