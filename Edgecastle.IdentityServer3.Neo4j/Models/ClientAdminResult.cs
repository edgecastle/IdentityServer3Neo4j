using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Edgecastle.IdentityServer3.Neo4j.Models
{
    /// <summary>
    /// Represents the result of a client operation
    /// </summary>
    public class ClientAdminResult
    {
        /// <summary>
        /// Initialises a new <see cref="ClientAdminResult"/> representing an error state.
        /// </summary>
        /// <param name="errorMessage"></param>
        public ClientAdminResult(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
            this.Success = false;
        }

        /// <summary>
        /// Initialises a new <see cref="ClientAdminResult"/> in a success state, providing the <see cref="Client"/>.
        /// </summary>
        /// <param name="client"></param>
        public ClientAdminResult(Client client)
        {
            this.Client = client;
            this.Success = true;
        }

        /// <summary>
        /// The client
        /// </summary>
        public Client Client { get; private set; }

        /// <summary>
        /// Whether the operation was a success or not
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// The error message
        /// </summary>
        public string ErrorMessage { get; private set; }
    }
}
