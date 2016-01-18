using System;

namespace Edgecastle.IdentityServer3.Neo4j.Models
{
    internal class AuthenticationInfo
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}