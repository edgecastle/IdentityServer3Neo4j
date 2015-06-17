using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;

namespace Edgecastle.IdentityServer3.Neo4j.Models
{
	/// <summary>
	/// Represents a claim. Serializable for storage in a Graph
	/// </summary>
	public class Claim
	{
		/// <summary>
		/// The claim type.
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// The claim value
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// The claim value's type
		/// </summary>
		public string ValueType { get; set; } = ClaimValueTypes.String;

		/// <summary>
		/// Implicit conversion between this <see cref="Claim"/> type and the <see cref="System.Security.Claims.Claim"/> type.
		/// </summary>
		/// <param name="c"></param>
		public static implicit operator System.Security.Claims.Claim (Claim c)
		{
			return Claim.ConvertToSystemClaim(c);
		}

		/// <summary>
		/// Implicit conversion between <see cref="System.Security.Claims.Claim"/> type and this <see cref="Claim"/> type.
		/// </summary>
		/// <param name="c"></param>
		public static implicit operator Claim (System.Security.Claims.Claim c)
		{
			return Claim.ConvertToPrimitiveClaim(c);
		}

		private static System.Security.Claims.Claim ConvertToSystemClaim(Claim c)
		{
			return new System.Security.Claims.Claim(c.Type, c.Value, c.ValueType);
		}

		private static Claim ConvertToPrimitiveClaim(System.Security.Claims.Claim c)
		{
			return new Claim
			{
				Type = c.Type,
				Value = c.Value,
				ValueType = c.ValueType
			};
		}
	}
}
