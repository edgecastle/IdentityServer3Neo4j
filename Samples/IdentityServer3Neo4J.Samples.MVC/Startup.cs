using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using IdentityServer3.Core.Configuration;
using System.Security.Cryptography.X509Certificates;
using Edgecastle.IdentityServer3.Neo4j;
using Microsoft.Owin.Security.OpenIdConnect;
using IdentityServer3.Core.Logging;
using Microsoft.Owin.Security.Cookies;
using IdentityServer3.Core;
using System.Web.Helpers;
using System.IdentityModel.Tokens;
using System.Collections.Generic;
using Serilog;
using Microsoft.Owin.Security.Facebook;

[assembly: OwinStartup(typeof(IdentityServer3Neo4J.Samples.MVC.Startup))]

namespace IdentityServer3Neo4J.Samples.MVC
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
            Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.Trace()
                            .CreateLogger();

            app.Map("/identity", idsrvApp =>
			{
				idsrvApp.UseIdentityServer(new IdentityServerOptions
				{
					SiteName = "Neo4j MVC Sample Auth Server",
					SigningCertificate = LoadCertificate(),

					// Reference the Neo4j version of the services factory
					Factory = Neo4jServiceFactory.Create(),

                    // Add external identity providers
                    AuthenticationOptions = new AuthenticationOptions
                    {
                        IdentityProviders = ConfigureIdentityProviders
                    }
				});
			});

			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationType = "Cookies"
			});

			app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
			{
				Authority = "https://localhost:44300/identity",
				ClientId = "mvcsample",
				Scope = "openid profile roles",
				RedirectUri = "https://localhost:44300/",
				ResponseType = "id_token",

				SignInAsAuthenticationType = "Cookies"
			});

			AntiForgeryConfig.UniqueClaimTypeIdentifier = Constants.ClaimTypes.Subject;
			JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();			
        }

        /// <summary>
        /// Configures external identity providers (such as Facebook, Google, Twitter, etc.)
        /// </summary>
        /// <param name="app">The OWIN app</param>
        /// <param name="signInAsType">The sign in type</param>
        public static void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var facebook = new FacebookAuthenticationOptions
            {
                AuthenticationType = "Facebook",
                Caption = "Facebook",
                SignInAsAuthenticationType = signInAsType,
                AppId = "...",
                AppSecret = "..."
            };
            app.UseFacebookAuthentication(facebook);
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
