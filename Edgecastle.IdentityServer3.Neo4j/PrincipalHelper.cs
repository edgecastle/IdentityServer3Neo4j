using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityModel.Extensions;

namespace Edgecastle.IdentityServer3.Neo4j
{
	/// <summary>
	/// Helpers for operations on the <see cref="ClaimsPrincipal"/>.
	/// </summary>
	public static class PrincipalHelper
	{
		/// <summary>
		/// Creates a claims principal from a set of claims
		/// </summary>
		/// <param name="subject">The subject claim (usually the name)</param>
		/// <param name="displayName">The friendly name of the subject</param>
		/// <param name="authenticationMethod">How the authentication occured</param>
		/// <param name="idp">The identity provider</param>
		/// <param name="authenticationType">The type of authentication</param>
		/// <param name="authenticationTime">The time the authentication happened</param>
		/// <returns></returns>
		public static ClaimsPrincipal Create(
			string subject,
			string displayName,
			string authenticationMethod = Constants.AuthenticationMethods.Password,
			string idp = Constants.BuiltInIdentityProvider,
			string authenticationType = Constants.PrimaryAuthenticationType,
			long authenticationTime = 0)
		{
			if (String.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException("subject");
			if (String.IsNullOrWhiteSpace(displayName)) throw new ArgumentNullException("displayName");
			if (String.IsNullOrWhiteSpace(authenticationMethod)) throw new ArgumentNullException("authenticationMethod");
			if (String.IsNullOrWhiteSpace(idp)) throw new ArgumentNullException("idp");
			if (String.IsNullOrWhiteSpace(authenticationType)) throw new ArgumentNullException("authenticationType");

			if (authenticationTime <= 0) authenticationTime = DateTimeOffset.UtcNow.ToEpochTime();

			var claims = new List<Claim>
			{
				new Claim(Constants.ClaimTypes.Subject, subject),
				new Claim(Constants.ClaimTypes.Name, displayName),
				new Claim(Constants.ClaimTypes.AuthenticationMethod, authenticationMethod),
				new Claim(Constants.ClaimTypes.IdentityProvider, idp),
				new Claim(Constants.ClaimTypes.AuthenticationTime, authenticationTime.ToString(), ClaimValueTypes.Integer)
			};

			var id = new ClaimsIdentity(claims, authenticationType, Constants.ClaimTypes.Name, Constants.ClaimTypes.Role);
			return new ClaimsPrincipal(id);
		}

		/// <summary>
		/// Creates a locally-well-understood principal from a standard <see cref="ClaimsPrincipal"/>.
		/// </summary>
		/// <param name="principal">The source principal</param>
		/// <param name="authenticationType">The authentication type</param>
		/// <returns></returns>
		public static ClaimsPrincipal CreateFromPrincipal(ClaimsPrincipal principal, string authenticationType)
		{
			// we require the following claims
			var subject = principal.FindFirst(Constants.ClaimTypes.Subject);
			if (subject == null) throw new InvalidOperationException("sub claim is missing");

			var name = principal.FindFirst(Constants.ClaimTypes.Name);
			if (name == null) throw new InvalidOperationException("name claim is missing");

			var authenticationMethod = principal.FindFirst(Constants.ClaimTypes.AuthenticationMethod);
			if (authenticationMethod == null) throw new InvalidOperationException("amr claim is missing");

			var authenticationTime = principal.FindFirst(Constants.ClaimTypes.AuthenticationTime);
			if (authenticationTime == null) throw new InvalidOperationException("auth_time claim is missing");

			var idp = principal.FindFirst(Constants.ClaimTypes.IdentityProvider);
			if (idp == null) throw new InvalidOperationException("idp claim is missing");

			var id = new ClaimsIdentity(principal.Claims, authenticationType, Constants.ClaimTypes.Name, Constants.ClaimTypes.Role);
			return new ClaimsPrincipal(id);
		}

		/// <summary>
		/// Creates a <see cref="ClaimsPrincipal"/> from a subject identifier (usually the name or username)
		/// </summary>
		/// <param name="subjectId">The subject identifier</param>
		/// <param name="additionalClaims">Additional claims to add to the principal</param>
		/// <returns></returns>
		public static ClaimsPrincipal FromSubjectId(string subjectId, IEnumerable<Claim> additionalClaims = null)
		{
			var claims = new List<Claim>
			{
				new Claim(Constants.ClaimTypes.Subject, subjectId)
			};

			if (additionalClaims != null)
			{
				claims.AddRange(additionalClaims);
			}

			return Principal.Create(Constants.PrimaryAuthenticationType,
				claims.Distinct(new ClaimComparer()).ToArray());
		}

		/// <summary>
		/// Creates a principal from a claims set
		/// </summary>
		/// <param name="claims">The claims to use to create the principal</param>
		/// <param name="allowMissing">Whether to allow any of the standard claims to be missing.</param>
		/// <returns></returns>
		public static ClaimsPrincipal FromClaims(IEnumerable<Claim> claims, bool allowMissing = false)
		{
			var sub = claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject);
			var amr = claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.AuthenticationMethod);
			var idp = claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.IdentityProvider);
			var authTime = claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.AuthenticationTime);

			var id = new ClaimsIdentity(Constants.BuiltInIdentityProvider);

			if (sub != null)
			{
				id.AddClaim(sub);
			}
			else
			{
				if (allowMissing == false)
				{
					throw new InvalidOperationException("sub claim is missing");
				}
			}

			if (amr != null)
			{
				id.AddClaim(amr);
			}
			else
			{
				if (allowMissing == false)
				{
					throw new InvalidOperationException("amr claim is missing");
				}
			}

			if (idp != null)
			{
				id.AddClaim(idp);
			}
			else
			{
				if (allowMissing == false)
				{
					throw new InvalidOperationException("idp claim is missing");
				}
			}

			if (authTime != null)
			{
				id.AddClaim(authTime);
			}
			else
			{
				if (allowMissing == false)
				{
					throw new InvalidOperationException("auth_time claim is missing");
				}
			}

			return new ClaimsPrincipal(id);
		}
	}
}