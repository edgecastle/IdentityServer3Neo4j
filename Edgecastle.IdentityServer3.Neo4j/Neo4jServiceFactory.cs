using Edgecastle.Data.Neo4j;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;

namespace Edgecastle.IdentityServer3.Neo4j
{
	/// <summary>
	/// Factory that registers and returns services that are backed by a Neo4j graph database
	/// </summary>
	public static class Neo4jServiceFactory
	{
		/// <summary>
		/// Creates a new instance of a Neo4j-backed service factory
		/// </summary>
		/// <returns>A Neo4j-backed service factory</returns>
		public static IdentityServerServiceFactory Create()
		{
			IdentityServerServiceFactory factory = new IdentityServerServiceFactory();

			factory.UserService = new Registration<IUserService, Neo4jUsersService>();
			factory.ClientStore = new Registration<IClientStore>(typeof(Neo4jClientStore));
			factory.ScopeStore = new Registration<IScopeStore>(typeof(Neo4jScopeStore));

            ConfigureDB();

			return factory;
		}

        private static void ConfigureDB()
        {
            var DB = Neo4jProvider.GetClient();

            try
            {
                string clientConstraintIdentity = string.Format("client:{0}", Configuration.Global.ClientLabel);
                DB.Cypher
                        .CreateUniqueConstraint(clientConstraintIdentity, "client.ClientId")
                        .ExecuteWithoutResults();

                string userConstraintIdentity = string.Format("user:{0}", Configuration.Global.UserLabel);
                DB.Cypher
                    .CreateUniqueConstraint(userConstraintIdentity, "user.Username")
                    .ExecuteWithoutResults();

                string scopeConstraintIdentity = string.Format("scope:{0}", Configuration.Global.ScopeLabel);
                DB.Cypher
                    .CreateUniqueConstraint(scopeConstraintIdentity, "scope.Name")
                    .ExecuteWithoutResults();

                string scopeClaimConstraintIdentity = string.Format("scopeClaim:{0}", Configuration.Global.ScopeClaimLabel);
                DB.Cypher
                    .CreateUniqueConstraint(scopeClaimConstraintIdentity, "scopeClaim.Name")
                    .ExecuteWithoutResults();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.Fail(ex.Message);
                throw;
            }
        }
    }
}
