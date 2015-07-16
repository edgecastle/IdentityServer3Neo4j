using Edgecastle.Data.Neo4j;
using Edgecastle.IdentityServer3.Neo4j.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Services;

namespace Edgecastle.IdentityServer3.Neo4j
{
	/// <summary>
	/// A scope store backed by a Neo4j graph database
	/// </summary>
	public class Neo4jScopeStore : IScopeStore
	{
		private GraphClient DB = Neo4jProvider.GetClient();

		private async Task<List<Thinktecture.IdentityServer.Core.Models.Scope>> GetAllScopes(Expression<Func<Scope,bool>> whereClause)
		{
			var scopes = new List<Thinktecture.IdentityServer.Core.Models.Scope>();

			var query = DB.Cypher
							.Match("(s:Scope)")
							.Where(whereClause)
							.Return(s => s.As<Scope>());

			var results = await query.ResultsAsync;

			if (results.Count() != 0)
			{
				scopes.AddRange(results.Select(s => (Thinktecture.IdentityServer.Core.Models.Scope)s));
			}
			scopes.AddRange(Thinktecture.IdentityServer.Core.Models.StandardScopes.All);
			return scopes;
		}

		/// <summary>
		/// Finds scopes by their names
		/// </summary>
		/// <param name="scopeNames">The names of the scopes</param>
		/// <returns>A collection of <see cref="Thinktecture.IdentityServer.Core.Models.Scope"/> objects appropriate to the names passed in.</returns>
		public async Task<IEnumerable<Thinktecture.IdentityServer.Core.Models.Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
		{
			return await GetAllScopes(s => scopeNames.ToList().Contains(s.Name));
		}

		/// <summary>
		/// Gets the scopes set, optionally excluding or including public-only scopes
		/// </summary>
		/// <param name="publicOnly">Whether to include or exclude non-public scopes</param>
		/// <returns>A collection of public scopes.</returns>
		public async Task<IEnumerable<Thinktecture.IdentityServer.Core.Models.Scope>> GetScopesAsync(bool publicOnly = true)
		{
			return await GetAllScopes(s => s.ShowInDiscoveryDocument);
		}
	}
}
