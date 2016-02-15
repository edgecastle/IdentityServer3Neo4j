using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edgecastle.Data.Neo4j
{
	/// <summary>
	/// Allows configuration of the labels used for different objects in the graph DB
	/// </summary>
	public class Configuration
	{
		private static Configuration _singleton = null;

		private NameValueCollection Settings = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="Configuration"/> class.
		/// </summary>
		/// <param name="settings">The settings used to define the labels</param>
		public Configuration(NameValueCollection settings)
		{
			this.Settings = settings ?? ConfigurationManager.AppSettings;

			if(this.Settings == null)
			{
				throw new ApplicationException("Unable to find or read Neo4j configuration");
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Configuration"/> class.
		/// </summary>
		public Configuration() : this(null) { }

		/// <summary>
		/// The connection string to use to connect to the graph databases
		/// </summary>
		public string ConnectionString
		{
			get
			{
				return Settings["Neo4jConnectionString"] ?? "http://neo4j:neo4j@localhost:7474/db/data";				
			}
		}

		/// <summary>
		/// The label applied to User objects (nodes) in the graph database
		/// </summary>
		public string UserLabel
		{
			get
			{
				return Settings["UserLabel"] ?? "User";
			}
		}
        
        /// <summary>
        /// The label applied to the Claim objects (nodes) in the graph database
        /// </summary>
        public string ClaimLabel
        {
            get
            {
                return Settings["ClaimLabel"] ?? "Claim";
            }
        }

		/// <summary>
		/// The label applied to External Login objects (nodes) in the graph database
		/// </summary>
		public string ExternalLoginLabel
		{
			get
			{
				return Settings["ExternalLoginLabel"] ?? "ExternalLogin";
			}
		}

		/// <summary>
		/// The label applied to Role objects (nodes) in the graph database
		/// </summary>
		public string RoleLabel
		{
			get
			{
				return Settings["RoleLabel"] ?? "Role";
			}
		}

		/// <summary>
		/// The name used to identify this provider. 
		/// </summary>
		public string AuthProviderName
		{
			get
			{
				return Settings["AuthProviderName"] ?? "IdentityServer3";
			}
		}

        /// <summary>
        /// The label applied to scopes in the graph database
        /// </summary>
        public string ScopeLabel
        {
            get
            {
                return Settings["ScopeLabel"] ?? "Scope";
            }
        }

        /// <summary>
        /// The label applied to scope claims in the graph database
        /// </summary>
        public string ScopeClaimLabel
        {
            get
            {
                return Settings["ScopeClaimLabel"] ?? "ScopeClaim";
            }
        }

        /// <summary>
        /// The label applied to Client Scope nodes in the graph database
        /// </summary>
        public string ClientScopeLabel
        {
            get
            {
                return Settings["ClientScopeLabel"] ?? "ClientScope";
            }
        }

        /// <summary>
        /// The label applied to client secrets
        /// </summary>
        public string ClientSecretLabel
        {
            get
            {
                return Settings["ClientSecretLabel"] ?? "ClientSecret";
            }
        }

        /// <summary>
        /// The label applied to Clients in the graph database
        /// </summary>
        public string ClientLabel
        {
            get
            {
                return Settings["ClientLabel"] ?? "Client";
            }
        }

        /// <summary>
        /// The name of the relationship between a subject and a claim
        /// </summary>
        public string HasClaimRelName
        {
            get
            {
                return Settings["HasClaimRelName"] ?? "HAS_CLAIM";
            }
        }

        /// <summary>
        /// The name of the relationship between a Client and a ClientSecret
        /// </summary>
        public string HasSecretRelName
        {
            get
            {
                return Settings["HasSecretRelName"] ?? "HAS_SECRET";
            }
        }

		/// <summary>
		/// Global singleton for accessing common graph configuration settings
		/// </summary>
		public static Configuration Global
		{
			get
			{
				if (_singleton == null)
				{
					_singleton = new Configuration();
				}

				return _singleton;
			}
		}
	}
}
