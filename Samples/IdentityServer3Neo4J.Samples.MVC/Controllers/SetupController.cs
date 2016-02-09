using Edgecastle.IdentityServer3.Neo4j;
using Edgecastle.IdentityServer3.Neo4j.Interfaces;
using Edgecastle.IdentityServer3.Neo4j.Models;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IdentityServer3Neo4J.Samples.MVC.Controllers
{
    public class SetupController : Controller
    {
        // GET: Setup
        public async Task<ActionResult> Index()
		{
			Log("Setting up Graph DB...");
            
			// Clients
			Log("Creating client for sample app...");
			await CreateClientsAsync();

            // Create a test user
			Log("Creating user 'bob' with password 'secret'");
			UserAdminResult createUserResult = await CreateTestUser();

            if (createUserResult.Success)
            {
                // Adding claims to user
                Log("Creating Claims and adding to user 'bob'");
                await AddClaimsToUserAsync(createUserResult.User);
            }

            // Creating 'roles' scope
			Log("Create 'roles' identity scope");
			await CreateRolesScope();

			return Content("-END-");
		}

		private async Task CreateRolesScope()
		{
			Edgecastle.IdentityServer3.Neo4j.Models.Scope newScope = new Edgecastle.IdentityServer3.Neo4j.Models.Scope
			{
				IsEnabled = true,
				Name = "roles",
				Type = ScopeType.Identity
			};

            IScopeAdminService service = new Neo4jScopeStore();
            ScopeAdminResult scopeResult = await service.CreateScope(newScope);
            if (scopeResult.Success)
            {
                Log("Created scope.");

                try
                {
                    var scopeClaims = new[]
                    {
                        new ScopeClaim("role")
                    };

                    foreach (var scopeClaim in scopeClaims)
                    {
                        Log("Adding scope claim '" + scopeClaim.Name + "'");
                        ScopeAdminResult result = await service.AddScopeClaim(newScope.Name, scopeClaim);
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }
		}

        private async Task CreateWebApiScope()
        {
            Log("Creating webapi resource scope.");

            Edgecastle.IdentityServer3.Neo4j.Models.Scope newScope = new Edgecastle.IdentityServer3.Neo4j.Models.Scope
            {
                IsEnabled = true,
                Name = "webapi",
                Type = ScopeType.Resource,
                Description = "Access to sample web API"
            };

            IScopeAdminService service = new Neo4jScopeStore();
            ScopeAdminResult scopeResult = await service.CreateScope(newScope);
            if (scopeResult.Success)
            {
                Log("Created web api resource scope.");                
            }
        }

		private async Task<UserAdminResult> CreateTestUser()
		{
            UserAdminResult result = null;

            try
			{
                IUserAdminService service = new Neo4jUsersService();
                result = await service.CreateUser(
                    username: "bob",
                    password: "secret",
                    email: "bob@smithventures.com"
                );

                if (result.Success)
                {
                    Log("Successfully created user.");
                }
                else
                {
                    Log(string.Format("Error: {0}", result.ErrorMessage), true);
                }

			}
			catch (Exception ex)
			{
				LogException(ex);
			}

            return result;
		}

		private async Task AddClaimsToUserAsync(User user)
		{
			try
			{
                IUserAdminService service = new Neo4jUsersService();

				var claims = new[]
				{
					new Claim { Type = Constants.ClaimTypes.Name, Value = "Bob Smith" },
					new Claim { Type = Constants.ClaimTypes.GivenName, Value = "Bob" },
					new Claim { Type = Constants.ClaimTypes.FamilyName, Value = "Smith" },
					new Claim { Type = Constants.ClaimTypes.Email, Value = "BobSmith@email.com" },
					new Claim { Type = Constants.ClaimTypes.EmailVerified, Value = "true", ValueType = System.Security.Claims.ClaimValueTypes.Boolean },
					new Claim { Type = Constants.ClaimTypes.Role, Value = "Developer" },
					new Claim { Type = Constants.ClaimTypes.Role, Value = "Geek"},
					new Claim { Type = Constants.ClaimTypes.WebSite, Value = "http://bob.com" },
					new Claim { Type = Constants.ClaimTypes.Address, Value = "{ \"street_address\": \"One Hacker Way\", \"locality\": \"Heidelberg\", \"postal_code\": 69118, \"country\": \"Germany\" }" }
				};

				foreach (var claim in claims)
				{
					try
					{
						Log(String.Format("Adding claim: {0} = {1}", claim.Type, claim.Value));

                        UserAdminResult result = await service.AddClaimToUser(user.Id, claim);
                        Log(String.Format("Success = {0}, ErrorMessage = {1}", result.Success, result.ErrorMessage ?? ""));
					}
					catch(Exception ex)
					{
						LogException(ex);
					}
				}

				Log("Done.");
			}
			catch (Exception ex)
			{
				LogException(ex);
			}
		}

		private async Task CreateClientsAsync()
		{
            IClientAdminService service = new Neo4jClientStore();
            ClientAdminResult result = null;

            try
            {
                Log("Creating MVC Application client scope.");



                var mvcClient = new Client
                {
                    Enabled = true,
                    ClientName = "MVC Sample",
                    ClientId = "mvcsample",
                    Flow = Flows.Implicit,
                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "profile",
                        "roles",
                        "webapi"
                    },
                    RedirectUris = new List<string>
                    {
                        "https://localhost:44300/"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "https://localhost:44300/"
                    }
                };

                result = await service.CreateClient(mvcClient);

                if (result.Success)
                {
                    Log("Done");
                }
                else
                {
                    Log("ERROR: " + result.ErrorMessage, true);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            try
            { 
                Log("Creating WebAPI Sample client scope.");

                var webapiClient = new Client
                {
                    ClientName = "WebApi Sample",
                    ClientId = "mvc_service",
                    Flow = Flows.ClientCredentials,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedScopes = new List<string>
                    {
                        "webapi"
                    }
                };

                result = await service.CreateClient(webapiClient);

                if(result.Success)
                {
                    Log("Done");
                }
                else
                {
                    Log("ERROR: " + result.ErrorMessage, true);
                }
			}
			catch (Exception ex)
			{
				LogException(ex);
			}

            try
            {
                Log("Creating JS Client scope.");

                var jsClient = new Client
                {
                    ClientName = "JS Client",
                    ClientId = "js",
                    Flow = Flows.Implicit,

                    RedirectUris = new List<string>
                    {
                        "https://localhost:44300/JsClient/Popup"
                    },

                    AllowedCorsOrigins = new List<string>
                    {
                        "https://localhost:44300"
                    },

                    AllowAccessToAllScopes = true
                };

                result = await service.CreateClient(jsClient);

                if (result.Success)
                {
                    Log("Done");
                }
                else
                {
                    Log("ERROR: " + result.ErrorMessage, true);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

		private void Log(string message, bool IsError = false)
		{
			Response.Write("<p" + (IsError ? " style='color:red'>" : ">") + HttpUtility.HtmlEncode(message) + "</p>");
			Response.Flush();
		}

		private void LogException(Exception e)
		{
			Exception ex = e;
            Log(ex.Message, true);

            if (ex is AggregateException)
            {
                AggregateException aggregateException = ex as AggregateException;
                foreach (Exception aggregateInnerEx in aggregateException.InnerExceptions)
                {
                    LogException(aggregateInnerEx);
                }
            }
            else
            {
                while (ex != null)
                {
                    this.Log(ex.Message, true);
                    ex = ex.InnerException;
                }
            }
		}
    }
}