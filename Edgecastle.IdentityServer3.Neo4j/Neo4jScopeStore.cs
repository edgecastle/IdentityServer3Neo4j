using Edgecastle.Data.Neo4j;
using Edgecastle.IdentityServer3.Neo4j.Interfaces;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Models;

namespace Edgecastle.IdentityServer3.Neo4j
{
	/// <summary>
	/// A scope store backed by a Neo4j graph database
	/// </summary>
	public class Neo4jScopeStore : IScopeStore, IScopeAdminService
	{
		private GraphClient DB = Neo4jProvider.GetClient();
		private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

		private async Task<List<Scope>> GetAllScopes()
		{
			return await this.GetAllScopes(null);
		}

		private async Task<List<Scope>> GetAllScopes(Expression<Func<Models.Scope,bool>> whereClause)
		{
			Logger.DebugFormat("GetAllScopes, whereClause = {0}", whereClause.ToString());

			var scopes = new List<Scope>();

            // TODO: Optional match on scope claim?
			var query = DB.Cypher
							.Match("(s:Scope)")
                            .OptionalMatch("(s)-[:HAS_CLAIM]->(sc:ScopeClaim)")
							.Return((s, sc) => new
							 {
								 Scope = s.As<Models.Scope>(),
								 ScopeClaims = sc.CollectAs<ScopeClaim>()
							 });

			// Support filtering
			if (whereClause != null)
			{
				query.Where(whereClause);
			}

			var results = await query.ResultsAsync;

			Logger.DebugFormat("GetAllScopes, found {0} scopes in the DB", results.Count());

			if (results.Any())
			{
                scopes.AddRange(results.Select(s => s.Scope.ToIdentityServerScope(s.ScopeClaims.ToList())));
			}

			scopes.AddRange(StandardScopes.All);

			Logger.DebugFormat("GetAllScopes, found total {0} scopes.", scopes.Count);

			return scopes;
		}

		/// <summary>
		/// Finds scopes by their names
		/// </summary>
		/// <param name="scopeNames">The names of the scopes</param>
		/// <returns>A collection of <see cref="Scope"/> objects appropriate to the names passed in.</returns>
		public async Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
		{
			return await GetAllScopes(s => scopeNames.ToList().Contains(s.Name));
		}

		/// <summary>
		/// Gets the scopes set, optionally excluding or including public-only scopes
		/// </summary>
		/// <param name="publicOnly">Whether to include or exclude non-public scopes</param>
		/// <returns>A collection of public scopes.</returns>
		public async Task<IEnumerable<Scope>> GetScopesAsync(bool publicOnly = true)
		{
			if (publicOnly)
			{
				return await GetAllScopes(s => s.ShowInDiscoveryDocument == true);
			}

			return await GetAllScopes();
		}

        /// <summary>
        /// Creates a scope
        /// </summary>
        /// <param name="scope">The scope to create</param>
        /// <returns>A <see cref="Models.ScopeAdminResult"/> indicating success or failure.</returns>
        public async Task<Models.ScopeAdminResult> CreateScope(Models.Scope scope)
        {
            try
            {
                await DB.Cypher
                        .Create("(s:Scope {newScope})")
                        .WithParam("newScope", scope)
                        .ExecuteWithoutResultsAsync();
            }
            catch (NeoException ex)
            {
                return new Models.ScopeAdminResult(ex.Message);
            }

            return new Models.ScopeAdminResult(scope);
        }

        /// <summary>
        /// Adds a claim to a scope
        /// </summary>
        /// <param name="scopeName">The name of the scope to add the claim to</param>
        /// <param name="claim">The claim to add to the scope</param>
        /// <returns>A <see cref="Models.ScopeAdminResult"/> indicating success or failure.</returns>
        public async Task<Models.ScopeAdminResult> AddScopeClaim(string scopeName, ScopeClaim claim)
        {
            try
            {
                var results = await DB.Cypher
                                .Match("(s:Scope { Name: {scopeName} })")
                                .CreateUnique("(s)-[:HAS_CLAIM]->(sc:ScopeClaim {claim})")
                                .WithParams(new
                                {
                                    scopeName = scopeName,
                                    claim = claim
                                })
                                // TODO: Return all scope claims
                                .Return((s,sc) => new
                                {
                                    Scope = s.As<Models.Scope>(),
                                    Claims = sc.CollectAs<ScopeClaim>()
                                })
                                .ResultsAsync;

                // TODO: Null check
                Models.Scope scope = results.First().Scope;
                // TODO: Don't break the Laws of Demeter
                scope.Claims = results.First().Claims.ToArray();

                return new Models.ScopeAdminResult(scope);
            }
            catch (NeoException ex)
            {
                // TODO: Log
                return new Models.ScopeAdminResult(ex.Message);
            }
        }

        /// <summary>
        /// Adds claims to a scope
        /// </summary>
        /// <param name="scopeName">The name of the scope to add the claims to</param>
        /// <param name="claims">The claims to add</param>
        /// <returns></returns>
        public async Task<IEnumerable<Models.ScopeAdminResult>> AddScopeClaims(string scopeName, IEnumerable<ScopeClaim> claims)
        {
            List<Models.ScopeAdminResult> results = new List<Models.ScopeAdminResult>();

            foreach (ScopeClaim claim in claims)
            {
                results.Add(await this.AddScopeClaim(scopeName, claim));
            }

            return results;
        }
    }
}
