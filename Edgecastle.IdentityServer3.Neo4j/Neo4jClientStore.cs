using Edgecastle.Data.Neo4j;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Edgecastle.IdentityServer3.Neo4j
{
	/// <summary>
	/// A client store backed by a Neo4j graph database
	/// </summary>
	public class Neo4jClientStore : IClientStore
	{
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
