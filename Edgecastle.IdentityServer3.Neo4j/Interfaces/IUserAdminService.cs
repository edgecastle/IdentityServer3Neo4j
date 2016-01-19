using Edgecastle.IdentityServer3.Neo4j.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Edgecastle.IdentityServer3.Neo4j.Interfaces
{
    /// <summary>
    /// Implemented by types that provide user administration operations
    /// </summary>
    public interface IUserAdminService
    {
        /// <summary>
        /// Creates a user with the given attributes
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <param name="email">The user's email address</param>
        /// <returns>An <see cref="AuthenticateResult"/>.</returns>
        Task<UserAdminResult> CreateUser(string username, string password, string email);

        /// <summary>
        /// Adds a claim to a user
        /// </summary>
        /// <param name="userId">The unique system-readable identifier of the user to which to attach the claim</param>
        /// <param name="claim">The claim to attach to the user</param>
        /// <returns>An <see cref="UserAdminResult"/> indicating success or failure.</returns>
        Task<UserAdminResult> AddClaimToUser(Guid userId, Claim claim);

        /// <summary>
        /// Adds claims to a user
        /// </summary>
        /// <param name="userId">The unique system-readable identifier of the user to which to attach the claims</param>
        /// <param name="claims">The claims to attach to the user</param>
        /// <returns></returns>
        Task<UserAdminResult> AddClaimsToUser(Guid userId, IEnumerable<Claim> claims);
    }
}
