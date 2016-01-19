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
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Neo4jClient;
using Edgecastle.IdentityServer3.Neo4j.Interfaces;

namespace IdentityServer3Neo4J.Samples.MVC.Controllers
{
    public class SetupController : Controller
    {
        // GET: Setup
        public async Task<ActionResult> Index()
		{
			Log("Setting up Graph DB...");
			var DB = Neo4jProvider.GetClient();

			Log("Creating constraints");
			await CreateConstraintsAsync(DB);

			// Clients
			Log("Creating client for sample app...");
			await CreateClientsAsync(DB);

			Log("Creating user 'bob' with password 'secret'");
			await CreateTestUser(DB);

			Log("Creating Claims and adding to user 'bob'");
			await AddClaimsToUserAsync(DB);

			Log("Create 'roles' identity scope");
			await CreateRolesScope(DB);

			return Content("-END-");
		}

		private async Task CreateRolesScope(GraphClient DB)
		{
			Edgecastle.IdentityServer3.Neo4j.Models.Scope newScope = new Edgecastle.IdentityServer3.Neo4j.Models.Scope
			{
				IsEnabled = true,
				Name = "roles",
				Type = ScopeType.Identity
			};
			
			try
			{
				await DB.Cypher
					.Create("(s:Scope {newScope})")
					.WithParam("newScope", newScope)
					.ExecuteWithoutResultsAsync();
		    }
			catch(Exception ex)
			{
				LogException(ex);
			}

			try
			{
				var scopeClaims = new[]
				{
					new ScopeClaim("role")
				};

				foreach (var scopeClaim in scopeClaims)
				{
					Log("Adding scope claim '" + scopeClaim.Name + "'");
					await DB.Cypher
						.Match("(s:Scope { Name: {newScopeName} })")
						.Create("(s)-[:HAS_CLAIM]->(sc:ScopeClaim {scopeClaim})")
						.WithParams(new
						{
							newScopeName = newScope.Name,
							scopeClaim = scopeClaim
						})
						.ExecuteWithoutResultsAsync();
				}

			}
			catch (Exception ex)
			{
				LogException(ex);
			}
		}

		private async Task CreateTestUser(Neo4jClient.GraphClient DB)
		{
			try
			{
                IUserAdminService service = new Neo4jUsersService();
                await service.CreateUser(
                    username: "bob",
                    password: PasswordSecurity.Hash("secret"),
                    email: "bob@smithventures.com"
                );
			}
			catch (Exception ex)
			{
				LogException(ex);
			}
		}

		private async Task AddClaimsToUserAsync(Neo4jClient.GraphClient DB)
		{
			try
			{
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

						await DB.Cypher.Match("(u:User {Username:'bob'})")
							.CreateUnique("(u)-[:HAS_CLAIM]->(c:Claim {claim})")
							.WithParam("claim", claim)
							.ExecuteWithoutResultsAsync();
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

		private async Task CreateClientsAsync(Neo4jClient.GraphClient DB)
		{
			try
			{
				var newClient = new Client
				{
					Enabled = true,
					ClientName = "MVC Sample",
					ClientId = "mvcsample",
					Flow = Flows.Implicit,

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

		private async Task CreateConstraintsAsync(Neo4jClient.GraphClient DB)
		{
			try
			{
				await DB.Cypher
						.CreateUniqueConstraint("client:Client", "client.ClientId")
						.ExecuteWithoutResultsAsync();
				await DB.Cypher
					.CreateUniqueConstraint("user:User", "user.Username")
					.ExecuteWithoutResultsAsync();
				await DB.Cypher
					.CreateUniqueConstraint("scope:Scope", "scope.Name")
					.ExecuteWithoutResultsAsync();
				await DB.Cypher
					.CreateUniqueConstraint("scopeClaim:ScopeClaim", "scopeClaim.Name")
					.ExecuteWithoutResultsAsync();
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