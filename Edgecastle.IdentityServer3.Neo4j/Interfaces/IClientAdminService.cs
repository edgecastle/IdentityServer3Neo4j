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
    /// An interface for a type providing client administration operations
    /// </summary>
    public interface IClientAdminService
    {
        /// <summary>
        /// Creates a client
        /// </summary>
        /// <param name="client">The client to create</param>
        /// <returns>A <see cref="ClientAdminResult"/> representing success or failure.</returns>
        Task<ClientAdminResult> CreateClient(Client client);
    }
}
