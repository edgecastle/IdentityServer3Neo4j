using Edgecastle.Data.Neo4j;
using Edgecastle.IdentityServer3.Neo4j.Interfaces;
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
using Edgecastle.IdentityServer3.Neo4j.Models;

namespace Edgecastle.IdentityServer3.Neo4j
{
	/// <summary>
	/// Users service backed by Neo4j graph database
	/// </summary>
	public class Neo4jUsersService : IUserService, IUserAdminService
	{
		private GraphClient DB = null;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Neo4jUsersService"/> class.
		/// </summary>
		public Neo4jUsersService()
		{
			DB = Neo4jProvider.GetClient();
		}

        /// <summary>
        /// Adds claims to a user
        /// </summary>
        /// <param name="userId">The unique, system-readable identifier of the user</param>
        /// <param name="claims">The claims</param>
        /// <returns></returns>
        public async Task<UserAdminResult> AddClaimsToUser(Guid userId, IEnumerable<Models.Claim> claims)
        {
            foreach (var claim in claims)
            {

            }
        }

        /// <summary>
        /// Adds a claim to a user
        /// </summary>
        /// <param name="userId">The unique, system-readable identifier for the user</param>
        /// <param name="claim"></param>
        /// <returns></returns>
        public async Task<UserAdminResult> AddClaimToUser(Guid userId, Models.Claim claim)
        {
            try
            {
                var results  = await DB.Cypher.Match("(u:User {Id: {id}})")
                                .WithParam("id", userId)
                                .CreateUnique("(u)-[:HAS_CLAIM]->(c:Claim {claim})")
                                .WithParam("claim", claim)
                                .Return((u,c) => new
                                {
                                    User = u.As<User>(),
                                    Claims = c.CollectAs<Models.Claim>()
                                })
                                .ResultsAsync;

                User user = results.First().User;
                user.Claims = results.First().Claims.Select(cl => cl.Data);
                return new UserAdminResult(user);
            }
            catch (NeoException ex)
            {
                return new UserAdminResult(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="externalUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser, SignInMessage message)
		{
			// TODO: External providers as separate nodes
			var query = DB.Cypher
							.Match("(u:User { Provider: {provider}, ProviderId: {providerId}})")
							.WithParams(new
							{
								provider = externalUser.Provider,
								providerId = externalUser.ProviderId
							})
							.Return(u => u.As<Models.User>());

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

				user = new Models.User
				{
					Id = Guid.NewGuid(),
					Provider = externalUser.Provider,
					ProviderId = externalUser.ProviderId,
					Username = displayName,
					Claims = externalUser.Claims.Select(c => (Models.Claim) c) // Cast
				};

				DB.Cypher
					.CreateUnique("(newUser:User {user})")
					.WithParam("user", user)
					.ExecuteWithoutResults();
			}

			var result = new AuthenticateResult(user.Id.ToString(), user.Username);
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public async Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message)
		{
            var usernameQuery = DB.Cypher
                            .Match("(u:User {Username:{username}})")
                            .WithParam("username", username.ToLowerInvariant())
                            .Return(u => u.As<Models.AuthenticationInfo>());

            var authenticationInfo = (await usernameQuery.ResultsAsync).FirstOrDefault();

            if (authenticationInfo == null || !PasswordSecurity.Verify(input: password, hash: authenticationInfo.Password))
            {
                // Couldn't find user with that username and/or password
                return new AuthenticateResult("Authentication failed.");
            }

			return new AuthenticateResult(authenticationInfo.Id.ToString(), authenticationInfo.Username);
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="username">The username (must be unique)</param>
        /// <param name="password">The password</param>
        /// <param name="email">The user's email address</param>
        /// <returns></returns>
        public async Task<AuthenticateResult> CreateUser(string username, string password, string email)
        {
            // Normalisation
            username = username.ToLowerInvariant();

            var existingUser = await DB.Cypher
                                    .Match("(existing:User {Username: {username}})")
                                    .WithParam("username", username)
                                    .Return(existing => existing.As<Models.User>())
                                    .ResultsAsync;

            if(existingUser.Any())
            {
                return new AuthenticateResult("Username already exists");
            }

            var newUser = new Models.User
            {
                Username = username,
                Password = PasswordSecurity.Hash(password),
                Id = Guid.NewGuid()
            };
            await DB.Cypher.Create("(u:User {newUser})")
                    .WithParam("newUser", newUser)
                    .ExecuteWithoutResultsAsync();

            return new AuthenticateResult(
                subject: newUser.Id.ToString(),
                name: newUser.Username
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="requestedClaimTypes"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null)
		{
			// TODO: This is all temporary
			var query = DB.Cypher
							.Match("(u:User {Username: {username}})-[:HAS_CLAIM]->(c:Claim)")
							.WithParam("username", subject.Identity.Name)
							.Return(c => c.As<Models.Claim>());

			var results = await query.ResultsAsync;

			var claims = new List<Claim>{
				new Claim(Constants.ClaimTypes.Subject, subject.Identity.Name),
			};

			claims.AddRange(results.Select(c => (Claim) c));
			if (requestedClaimTypes != null)
			{
				claims = claims.Where(x => requestedClaimTypes.Contains(x.Type)).ToList();
			}

			return claims;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="subject"></param>
		/// <returns></returns>
		public Task<bool> IsActiveAsync(ClaimsPrincipal subject)
		{
			return Task.FromResult<bool>(true);
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
			return Task.FromResult(0);
		}

		/// <summary>
		/// Retrieves the display name.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <returns></returns>
		protected virtual string GetDisplayName(Models.User user)
		{
			throw new NotImplementedException("Need to read from db");

			/*var nameClaim = user.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name);
			if (nameClaim != null)
			{
				return nameClaim.Value;
			}
			return user.Username;*/
		}

        Task<UserAdminResult> IUserAdminService.CreateUser(string username, string password, string email)
        {
            throw new NotImplementedException();
        }
    }
}
