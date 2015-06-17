using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;

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

			return factory;
		}
	}
}
