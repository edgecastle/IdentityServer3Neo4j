using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Thinktecture.IdentityServer.Core.Configuration;
using System.Security.Cryptography.X509Certificates;
using Edgecastle.IdentityServer3.Neo4j;
using Microsoft.Owin.Security.OpenIdConnect;

[assembly: OwinStartup(typeof(IdentityServer3Neo4J.Samples.MVC.Startup))]

namespace IdentityServer3Neo4J.Samples.MVC
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.Map("/identity", idsrvApp =>
			{
				idsrvApp.UseIdentityServer(new IdentityServerOptions
				{
					SiteName = "Neo4j MVC Sample Auth Server",
					SigningCertificate = LoadCertificate(),

					// Reference the Neo4j version of the services factory
					Factory = Neo4jServiceFactory.Create(),

					// For use on VS2015 RC (which doesn't support SSL on IIS Express)
					RequireSsl = false,
				});
			});

			app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
			{
				Authority = "http://localhost:58065/identity",
				ClientId = "mvcsample",
				RedirectUri = "http://localhost:58065/",
				ResponseType = "id_token",

				SignInAsAuthenticationType = "Cookies",

				// Disable RequiresNonce as not implemented yet
				ProtocolValidator = new Microsoft.IdentityModel.Protocols.OpenIdConnectProtocolValidator { RequireNonce = false }
            });
        }

		/// <summary>
		/// Loads the certificate used for signing and verifying tokens
		/// </summary>
		/// <returns>The certificate.</returns>
		/// <exception cref="ApplicationException">Thrown when the certificate can't be found. The certificate must be present in the Local Machine\Personal certificates store.</exception>
		X509Certificate2 LoadCertificate()
		{
			X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
			store.Open(OpenFlags.ReadOnly);
			X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindBySubjectName, "EdgecastleAuth", true);
			if(certificates.Count != 0)
			{
				return certificates[0];
			}

			throw new ApplicationException("Unable to find certificate.");
		}
	}
}
