using Edgecastle.Data.Neo4j;
using Edgecastle.IdentityServer3.Neo4j;
using Edgecastle.IdentityServer3.Neo4j.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using Edgecastle.IdentityServer3.Neo4j.Interfaces;

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
			User user = await CreateTestUser();

            // Adding claims to user
			Log("Creating Claims and adding to user 'bob'");
			await AddClaimsToUserAsync(user);

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

		private async Task<User> CreateTestUser()
		{
			try
			{
                IUserAdminService service = new Neo4jUsersService();
                UserAdminResult result = await service.CreateUser(
                    username: "bob",
                    password: "secret",
                    email: "bob@smithventures.com"
                );

                return result.Success ? result.User : null;
			}
			catch (Exception ex)
			{
				LogException(ex);
			}

            return null;
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
			try
			{
				var newClient = new Client
				{
					Enabled = true,
					ClientName = "MVC Sample",
					ClientId = "mvcsample",
					Flow = Flows.Implicit,
                    AllowedScopes = new List<string> { "openid", "profile", "roles" },
					RedirectUris = new List<string>
					{
						"https://localhost:44300/"
					}
				};

                IClientAdminService service = new Neo4jClientStore();
                var result = await service.CreateClient(newClient);

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
			Response.Write("<p" + (IsError ? " style='color:red'>" : ">") + message + "</p>");
			Response.Flush();
		}

		private void LogException(Exception e)
		{
			Exception ex = e;
			while(ex != null)
			{
				this.Log(ex.Message, true);
				ex = ex.InnerException;
			}
		}
    }
}