using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Edgecastle.IdentityServer3.Neo4j.Models
{
	/// <summary>
	/// Models a resource (either identity resource or web api resource)
	/// </summary>
	public class Scope
	{
		/// <summary>
		/// List of user claims that should be included in the identity (identity scope) or access token (resource scope).
		/// </summary>
		public ScopeClaim[] Claims { get; set; }

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
		public ScopeType Type { get; set; }

		/// <summary>
		/// Implicit casting to a Thinktecture IdentityServer3 scope
		/// </summary>
		/// <param name="scope">The Neo4j-compatible scope</param>
		public static implicit operator Thinktecture.IdentityServer.Core.Models.Scope (Scope scope)
		{
			return new Thinktecture.IdentityServer.Core.Models.Scope
			{
				Claims = scope.Claims.ToList(),
				ClaimsRule = scope.ClaimsRule,
				Description = scope.Description,
				DisplayName = scope.DisplayName,
				Emphasize = scope.Emphasize,
				Enabled = scope.IsEnabled,
				IncludeAllClaimsForUser = scope.IncludeAllClaimsForUser,
				Name = scope.Name,
				Required = scope.Required,
				ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument,
				Type = scope.Type
			};
		}
	}
}