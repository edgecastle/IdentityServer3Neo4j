using System.Web;
using System.Web.Mvc;

namespace IdentityServer3Neo4J.Samples.MVC
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}
