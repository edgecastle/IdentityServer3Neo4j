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
    /// Interface for a service that provides <see cref="Models.Scope"/> administrative operations
    /// </summary>
    public interface IScopeAdminService
    {
        /// <summary>
        /// Creates a scope
        /// </summary>
        /// <param name="scope">The scope to create</param>
        /// <returns></returns>
        Task<ScopeAdminResult> CreateScope(Models.Scope scope);

        /// <summary>
        /// Adds a claim to a scope
        /// </summary>
        /// <param name="scopeName">The scope to add the claim to</param>
        /// <param name="claim">The claim to add</param>
        /// <returns>A <see cref="ScopeAdminResult"/> indicating success or failure.</returns>
        Task<ScopeAdminResult> AddScopeClaim(string scopeName, ScopeClaim claim);

        /// <summary>
        /// Adds claims to a scope
        /// </summary>
        /// <param name="scopeName">The name of the scope to attach the claims to</param>
        /// <param name="claims">The claims to attach to the scope</param>
        /// <returns>A <see cref="ScopeAdminResult"/> indicating success or failure.</returns>
        Task<IEnumerable<ScopeAdminResult>> AddScopeClaims(string scopeName, IEnumerable<ScopeClaim> claims);
    }
}
