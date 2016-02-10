using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using IdentityModel;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace IdentityServer3Neo4J.Samples.MVC.Controllers
{
    public class CallApiController : Controller
    {
        public async Task<ActionResult> ClientCredentials()
        {
            var response = await GetTokenAsync();
            var result = await CallApi(response.AccessToken);

            ViewBag.Json = result;
            return View("ShowApiResult");
        }

        private async Task<TokenResponse> GetTokenAsync()
        {
            var client = new TokenClient(
                "https://localhost:44300/identity/connect/token",
                "mvc_service",
                "secret");

            return await client.RequestClientCredentialsAsync("webapi");
        }

        private async Task<string> CallApi(string token)
        {
            var client = new HttpClient();
            client.SetBearerToken(token);

            var json = await client.GetStringAsync("https://localhost:44301/identity");
            return JArray.Parse(json).ToString();
        }
    }
}