using Edgecastle.Data.Neo4j;
using Neo4jClient;
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
	/// A scope store backed by a Neo4j graph database
	/// </summary>
	public class Neo4jScopeStore : IScopeStore
	{
		private GraphClient DB = Neo4jProvider.GetClient();

		/// <summary>
		/// Finds scopes by their names
		/// </summary>
		/// <param name="scopeNames">The names of the scopes</param>
		/// <returns>A collection of <see cref="Scope"/> objects appropriate to the names passed in.</returns>
		public async Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
		{
			var scopes = new List<Scope>();

			var query = DB.Cypher
							.Match("(s:Scope)")
							.Return(s => s.As<Scope>());

			var results = await query.ResultsAsync;

			if(results.Count() != 0)
			{
				scopes.AddRange(results);
			}
			scopes.AddRange(StandardScopes.All);

			return scopes.Where(s => scopeNames.ToList().Contains(s.Name));
		}

		/// <summary>
		/// Gets the scopes set, optionally excluding or including public-only scopes
		/// </summary>
		/// <param name="publicOnly">Whether to include or exclude non-public scopes</param>
		/// <returns>A collection of <see cref="Scope"/> objects.</returns>
		public Task<IEnumerable<Scope>> GetScopesAsync(bool publicOnly = true)
		{
			return Task.FromResult<IEnumerable<Scope>>(StandardScopes.All);
		}
	}
}
