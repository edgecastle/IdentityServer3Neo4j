﻿using Edgecastle.Data.Neo4j;
using Edgecastle.IdentityServer3.Neo4j.Interfaces;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;

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
        public async Task<IEnumerable<Models.UserAdminResult>> AddClaimsToUser(Guid userId, IEnumerable<Models.Claim> claims)
        {
            List<Models.UserAdminResult> results = new List<Models.UserAdminResult>();

            foreach (var claim in claims)
            {
                results.Add(await this.AddClaimToUser(userId, claim));
            }

            return results;
        }

        /// <summary>
        /// Adds a claim to a user
        /// </summary>
        /// <param name="userId">The unique, system-readable identifier for the user</param>
        /// <param name="claim"></param>
        /// <returns></returns>
        public async Task<Models.UserAdminResult> AddClaimToUser(Guid userId, Models.Claim claim)
        {
            try
            {
                string matchString = string.Format("(u:{0} {{Id: {{id}}}})", Configuration.Global.UserLabel);
                string createUniqueString = string.Format("(u)-[:{0}]->(c:{1} {{claim}})",
                                                    Configuration.Global.HasClaimRelName,
                                                    Configuration.Global.ClaimLabel);

                var results  = await DB.Cypher.Match(matchString)
                                .WithParam("id", userId)
                                .CreateUnique(createUniqueString)
                                .WithParam("claim", claim)
                                .Return((u,c) => new
                                {
                                    User = u.As<Models.User>(),
                                    Claims = c.CollectAs<Models.Claim>()
                                })
                                .ResultsAsync;

                Models.User user = results.First().User;
                user.Claims = results.First().Claims;
                return new Models.UserAdminResult(user);
            }
            catch (NeoException ex)
            {
                return new Models.UserAdminResult(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
		{
            string matchString = string.Format("(u:{0} {{Provider: {{provider}}, ProviderId: {{providerId}}}})",
                                                Configuration.Global.UserLabel);

			// TODO: External providers as separate nodes
			var query = DB.Cypher
							.Match(matchString)
							.WithParams(new
							{
								provider = context.ExternalIdentity.Provider,
								providerId = context.ExternalIdentity.ProviderId
							})
							.Return(u => u.As<Models.User>());

			var user = (await query.ResultsAsync).FirstOrDefault();

			if(user == null)
			{
				string displayName;

				var name = context.ExternalIdentity.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name);
				if (name == null)
				{
					displayName = context.ExternalIdentity.ProviderId;
				}
				else
				{
					displayName = name.Value;
				}

				user = new Models.User
				{
					Id = Guid.NewGuid(),
					Provider = context.ExternalIdentity.Provider,
					ProviderId = context.ExternalIdentity.ProviderId,
					Username = displayName
				};

                // Create the user
                string createUserString = string.Format("(newUser:{0} {{user}})",
                                                    Configuration.Global.UserLabel);

				Models.User createdUser = (await DB.Cypher
					.Create(createUserString)
					.WithParam("user", user)
					.Return(newUser => newUser.As<Models.User>())
                    .ResultsAsync).Single();

                // Now add claims from the external provider
                await this.AddClaimsToUser(createdUser.Id, context.ExternalIdentity.Claims.Select(c => (Models.Claim)c));
			}

			context.AuthenticateResult = new AuthenticateResult(user.Id.ToString(), user.Username);
		}

		/// <summary>
		/// Authenticates the user with a local account
		/// </summary>
		/// <param name="context">The context</param>
		/// <returns></returns>
		public async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
		{
            string matchString = string.Format("(u:{0} {{Username:{{username}}}})",
                                                Configuration.Global.UserLabel);

            var usernameQuery = DB.Cypher
                            .Match(matchString)
                            .WithParam("username", context.UserName.ToLowerInvariant())
                            .Return(u => u.As<Models.AuthenticationInfo>());

            var authenticationInfo = (await usernameQuery.ResultsAsync).FirstOrDefault();

            if (authenticationInfo == null || !PasswordSecurity.Verify(input: context.Password, hash: authenticationInfo.Password))
            {
                // Couldn't find user with that username and/or password
                context.AuthenticateResult = new AuthenticateResult("Authentication failed.");
            }

			context.AuthenticateResult = new AuthenticateResult(authenticationInfo.Id.ToString(), authenticationInfo.Username);
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="username">The username (must be unique)</param>
        /// <param name="password">The password</param>
        /// <param name="email">The user's email address</param>
        /// <returns></returns>
        public async Task<Models.UserAdminResult> CreateUser(string username, string password, string email)
        {
            // Normalisation
            username = username.ToLowerInvariant();

            string matchString = string.Format("(existing:{0} {{Username: {{username}}}})",
                                                    Configuration.Global.UserLabel);

            var existingUser = await DB.Cypher
                                    .Match(matchString)
                                    .WithParam("username", username)
                                    .Return(existing => existing.As<Models.User>())
                                    .ResultsAsync;

            if(existingUser.Any())
            {
                return new Models.UserAdminResult("Username already exists");
            }

            var newUser = new Models.User
            {
                Username = username,
                Password = PasswordSecurity.Hash(password),
                Id = Guid.NewGuid()
            };

            string createUserString = string.Format("(u:{0} {{newUser}})",
                                            Configuration.Global.UserLabel);

            await DB.Cypher.Create(createUserString)
                    .WithParam("newUser", newUser)
                    .ExecuteWithoutResultsAsync();

            return new Models.UserAdminResult(newUser);
        }

        /// <summary>
        /// Populates the context with issues claims.
        /// </summary>
        /// <param name="context">The profile data request</param>
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)   // ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null)
		{
            string matchString = string.Format("(u:{0} {{Username: {{username}}}})-[:{1}]->(c:{2})",
                                                Configuration.Global.UserLabel,
                                                Configuration.Global.HasClaimRelName,
                                                Configuration.Global.ClaimLabel);

			// TODO: This is all temporary
			var query = DB.Cypher
							.Match(matchString)
							.WithParam("username", context.Subject.Identity.Name)
							.Return(c => c.As<Models.Claim>());

			var results = await query.ResultsAsync;

			var claims = new List<Claim>{
				new Claim(Constants.ClaimTypes.Subject, context.Subject.Identity.Name),
			};

			claims.AddRange(results.Select(c => (Claim) c));
			if (context.RequestedClaimTypes != null)
			{
				claims = claims.Where(x => context.RequestedClaimTypes.Contains(x.Type)).ToList();
			}

            context.IssuedClaims = claims;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public Task IsActiveAsync(IsActiveContext context)
		{
            return Task.FromResult(0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context">The context</param>
		/// <returns></returns>
		public Task PreAuthenticateAsync(PreAuthenticationContext context)
		{
            return Task.FromResult(0);
		}

        /// <summary>
        /// Post authentication
        /// </summary>
        /// <param name="context">The context</param>
        /// <returns></returns>
        public Task PostAuthenticateAsync(PostAuthenticationContext context)
        {
            return Task.FromResult(0);
        }

		/// <summary>
		/// Signs out
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public Task SignOutAsync(SignOutContext context)
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
    }
}
