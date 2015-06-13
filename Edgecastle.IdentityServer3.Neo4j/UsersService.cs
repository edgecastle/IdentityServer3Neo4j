using Edgecastle.Data.Neo4j;
using Edgecastle.IdentityServer3.Neo4j.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Edgecastle.IdentityServer3.Neo4j
{
	/// <summary>
	/// 
	/// </summary>
	public class UsersService : IUserService
	{
		private GraphClient DB = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="UsersService"/> class.
		/// </summary>
		public UsersService()
		{
			DB = Neo4jProvider.GetClient();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="externalUser"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public async Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser, SignInMessage message)
		{
			var query = DB.Cypher
							.Match("(user:User { Provider: {provider}, ProviderId: {providerId}})")
							.WithParams(new
							{
								provider = externalUser.Provider,
								providerId = externalUser.ProviderId
							})
							.Return(user => user.As<User>());

			var user = (await query.ResultsAsync).FirstOrDefault();

			if(user == null)
			{
				string displayName;

				var name = externalUser.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name);
				if (name == null)
				{
					displayName = externalUser.ProviderId;
				}
				else
				{
					displayName = name.Value;
				}

				user = new User
				{
					Subject = CryptoRandom.CreateUniqueId(),
					Provider = externalUser.Provider,
					ProviderId = externalUser.ProviderId,
					Username = displayName,
					Claims = externalUser.Claims
				};

				DB.Cypher
					.Create("(newUser:User {user})")
					.WithParam("user", user)
					.ExecuteWithoutResults();
			}

			var p = IdentityServerPrincipal.Create(user.Subject, GetDisplayName(user), Constants.AuthenticationMethods.External, user.Provider);
			var result = new AuthenticateResult(p);
			return Task.FromResult(result);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="subject"></param>
		/// <param name="requestedClaimTypes"></param>
		/// <returns></returns>
		public Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="subject"></param>
		/// <returns></returns>
		public Task<bool> IsActiveAsync(ClaimsPrincipal subject)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public Task<AuthenticateResult> PreAuthenticateAsync(SignInMessage message)
		{
			return Task.FromResult<AuthenticateResult>(null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="subject"></param>
		/// <returns></returns>
		public Task SignOutAsync(ClaimsPrincipal subject)
		{
			throw new NotImplementedException();
		}
	}
}
