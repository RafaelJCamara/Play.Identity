using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using System;
using System.Collections.Generic;

namespace Play.Identity.Service.Entities
{
    /*
        We use this attribute, because otherwise our collectionName would be applicationUser
        To better up the name of the collection, we use this attribute
     */
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        public decimal Gil { get; set; }
        public HashSet<Guid> MessageIds { get; set; } = new();
    }
}
