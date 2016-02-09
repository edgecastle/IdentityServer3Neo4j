using System;
using Microsoft.Owin;
using Owin;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using IdentityServer3.AccessTokenValidation;

[assembly: OwinStartup(typeof(IdentityServer3.Neo4J.Samples.WebApi.Startup))]

namespace IdentityServer3.Neo4J.Samples.WebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = "https://localhost:44300/identity",
                RequiredScopes = new[] { "webapi" }
            });

            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            app.UseWebApi(config);
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
            if (certificates.Count != 0)
            {
                return certificates[0];
            }

            throw new ApplicationException("Unable to find certificate.");
        }
    }
}
