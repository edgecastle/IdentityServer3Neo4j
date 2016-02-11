using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer = IdentityServer3.Core.Models;

namespace Edgecastle.IdentityServer3.Neo4j.Models
{
	/// <summary>
	/// Models a resource (either identity resource or web api resource)
	/// </summary>
	public class Scope
	{
		/// <summary>
		/// Converts a Neo4j-serializable Scope to the IdentityServer3 Scope object
		/// </summary>
		/// <param name="scopeClaims">A list of any scope claims to add</param>
		/// <returns>An IdentityServer3 scope representative of the Neo4j-serializable scope object.</returns>
		internal IdentityServer.Scope ToIdentityServerScope(List<IdentityServer.ScopeClaim> scopeClaims)
		{
			var idSrvScope = new IdentityServer.Scope
			{
				Claims = scopeClaims,
				ClaimsRule = this.ClaimsRule,
				Description = this.Description,
				DisplayName = this.DisplayName,
				Emphasize = this.Emphasize,
				Enabled = this.IsEnabled,
				IncludeAllClaimsForUser = this.IncludeAllClaimsForUser,
				Name = this.Name,
				Required = this.Required,
				ShowInDiscoveryDocument = this.ShowInDiscoveryDocument,
				Type = this.Type
			};

			return idSrvScope;
		}

		/// <summary>
		/// List of user claims that should be included in the identity (identity scope) or access token (resource scope).
		/// </summary>
		public IEnumerable<IdentityServer.ScopeClaim> Claims { get; set; }

		/// <summary>
		/// Rule for determining which claims should be included in the token (this is implementation specific)
		/// </summary>
		public string ClaimsRule { get; set; }

		/// <summary>
		/// Description. This value will be used e.g. on the consent screen.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Display name. This value will be used e.g. on the consent screen.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Specifies whether the consent screen will emphasize this scope. Use this setting for sensitive or important scopes. Defaults to false.
		/// </summary>
		public bool Emphasize { get; set; }

		/// <summary>
		/// Indicates if scope is enabled and can be requested. Defaults to true.
		/// </summary>
		public bool IsEnabled { get; set; }

		/// <summary>
		/// If enabled, all claims for the user will be included in the token. Defaults to false.
		/// </summary>
		public bool IncludeAllClaimsForUser { get; set; }

		/// <summary>
		/// Name of the scope. This is the value a client will use to request the scope.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Specifies whether the user can de-select the scope on the consent screen. Defaults to false.
		/// </summary>
		public bool Required { get; set; }

		/// <summary>
		/// Specifies whether this scope is shown in the discovery document. Defaults to true.
		/// </summary>
		public bool ShowInDiscoveryDocument { get; set; }

		/// <summary>
		/// Specifies whether this scope is about identity information from the userinfo endpoint, or a resource (e.g. a Web API). Defaults to Resource.
		/// </summary>
		public IdentityServer.ScopeType Type { get; set; }
	}
}