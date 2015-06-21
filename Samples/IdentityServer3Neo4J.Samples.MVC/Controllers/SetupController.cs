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
			try
			{
				var newUser = new User
				{
					Username = "bob",
					Password = "secret",
					Subject = "88421113",
				};
				await DB.Cypher.Create("(u:User {newUser})")
						.WithParam("newUser", newUser)
						.ExecuteWithoutResultsAsync();
			}
			catch (Exception ex)
			{
				LogException(ex);
			}

			Log("Creating Claims and adding to user 'bob'");
			await AddClaimsToUserAsync(DB);

			return Content("-END-");
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
							.CreateUnique("(u)-[:HAS_CLAIM]->(c:Claim {claim})") // Means we don't give the same claim twice
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

				await DB.Cypher.Create("(c:Client {newClient})")
						.WithParam("newClient", newClient)
						.ExecuteWithoutResultsAsync();

				Log("Done");
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