using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Edgecastle.IdentityServer3.Neo4j.Models
{
	/// <summary>
	/// Represents a user in the system
	/// </summary>
	public class User
	{
		/// <summary>
		/// Gets or sets the subject.
		/// </summary>
		/// <value>
		/// The subject.
		/// </value>
		public string Subject { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="User"/> is enabled.
		/// </summary>
		/// <value>
		///   <c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		public bool Enabled { get; set; }

		/// <summary>
		/// Gets or sets the username.
		/// </summary>
		/// <value>
		/// The username.
		/// </value>
		public string Username { get; set; }

		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		/// <value>
		/// The password.
		/// </value>
		public string Password { get; set; }

		/// <summary>
		/// Gets or sets the provider.
		/// </summary>
		/// <value>
		/// The provider.
		/// </value>
		public string Provider { get; set; }

		/// <summary>
		/// Gets or sets the provider identifier.
		/// </summary>
		/// <value>
		/// The provider identifier.
		/// </value>
		public string ProviderId { get; set; }

		/// <summary>
		/// Gets or sets the claims.
		/// </summary>
		/// <value>
		/// The claims.
		/// </value>
		public IEnumerable<Claim> Claims { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="User"/> class.
		/// </summary>
		public User()
		{
			Enabled = true;
			Claims = new List<Claim>();
		}
	}
}