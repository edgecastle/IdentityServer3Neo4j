using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer3Neo4j.Samples.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var token = GetUserToken();
            CallApi(token);

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static TokenResponse GetUserToken()
        {
            var client = new TokenClient(
                "https://localhost:44300/identity/connect/token",
                "consoleappsample",
                "secret");

            return client.RequestResourceOwnerPasswordAsync("bob", "secret", "webapi").Result;
        }

        static void CallApi(TokenResponse response)
        {
            var client = new HttpClient();
            client.SetBearerToken(response.AccessToken);

            Console.WriteLine(client.GetStringAsync("https://localhost:44301/identity").Result);
        }
    }
}
