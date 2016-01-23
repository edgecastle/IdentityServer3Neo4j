using Edgecastle.Data.Neo4j;
using Edgecastle.IdentityServer3.Neo4j.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Edgecastle.IdentityServer3.Neo4j.Models;

namespace Edgecastle.IdentityServer3.Neo4j
{
	/// <summary>
	/// A client store backed by a Neo4j graph database
	/// </summary>
	public class Neo4jClientStore : IClientStore, IClientAdminService
	{
        /// <summary>
        /// Creates a client
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public async Task<ClientAdminResult> CreateClient(Client client)
        {
            Client newClient = null;

            try
            {
                var DB = Neo4jProvider.GetClient();

                // Only copy user-settable fields (whitelisting)
                newClient = new Client
                {
                    Enabled = client.Enabled,
                    ClientName = client.ClientName,
                    ClientId = client.ClientId,
                    Flow = Flows.Implicit,
                    AllowedScopes = client.AllowedScopes,

                    // TODO - Uniqueness of redirect uris
                    RedirectUris = new List<string>
                    {
                        client.RedirectUris.FirstOrDefault()
                    }
                };

                await DB.Cypher.Create("(c:Client {newClient})")
                        .WithParam("newClient", newClient)
                        .ExecuteWithoutResultsAsync();
            }
            catch (Neo4jClient.NeoException ex)
            {
                // TODO: Log
                return new ClientAdminResult(ex.Message);
            }

            return new ClientAdminResult(newClient);
        }

        /// <summary>
        /// Finds a client by its identifier
        /// </summary>
        /// <param name="clientId">The unique identifier for the client</param>
        /// <returns>The client, if found, otherwise null.</returns>
        public async Task<Client> FindClientByIdAsync(string clientId)
		{
			var DB = Neo4jProvider.GetClient();

			var query = DB.Cypher
							.Match("(c:Client {ClientId:{clientId}})")
							.WithParam("clientId", clientId)
							.Return(c => c.As<Client>());

			var results = await query.ResultsAsync;

			return results.FirstOrDefault();
		}
	}
}
