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
                    Flow = client.Flow,
                    AllowedScopes = client.AllowedScopes,
                    PostLogoutRedirectUris = client.PostLogoutRedirectUris,
                    RedirectUris = client.RedirectUris,
                    AllowAccessToAllScopes = client.AllowAccessToAllScopes
                };

                if (client.ClientSecrets != null && client.ClientSecrets.Count != 0)
                {
                    // Prepare the create string using the configurable labels from Configuration
                    string createString = string.Format("(c:{0} {{newClient}})-[:{1}]->(cs:{2} {{secret}})",
                                                Configuration.Global.ClientLabel,
                                                Configuration.Global.HasSecretRelName,
                                                Configuration.Global.HasSecretRelName);


                    // TODO: Merge

                    await DB.Cypher.CreateUnique(createString)
                            .WithParams(new
                            {
                                newClient = newClient,
                                secret = client.ClientSecrets.First()
                            })
                            .ExecuteWithoutResultsAsync();
                }
                else
                {
                    // Prepare the create string using the configurable labels from Configuration
                    string createString = string.Format("(c:{0} {{newClient}})",
                                                Configuration.Global.ClientLabel);
                    await DB.Cypher.Create("(c:Client {newClient})")
                            .WithParam("newClient", newClient)
                            .ExecuteWithoutResultsAsync();
                }
                
            }
            catch (Exception ex)
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

            // Prepare the query strings using the configurable labels from Configuration
            string matchString = string.Format("(c:{0} {{ClientId:{{clientId}}}})",
                                        Configuration.Global.ClientLabel);

            string optionalMatchString = string.Format("(c)-[:{0}]->(cs:{1})",
                                        Configuration.Global.HasSecretRelName,
                                        Configuration.Global.ClientSecretLabel);

			var query = DB.Cypher
							.Match(matchString)
                            .OptionalMatch(optionalMatchString)
							.WithParam("clientId", clientId)
							.Return((c, cs) => new
                            {
                                Client = c.As<Client>(),
                                Secrets = cs.CollectAs<Secret>()
                            });

			var results = await query.ResultsAsync;

            Client client = results.Single().Client;
            client.ClientSecrets = results.Single().Secrets.Select(s => s.Data).ToList();

            return client;
		}
	}
}
