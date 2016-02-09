using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace IdentityServer3.Neo4J.Samples.WebApi.Controllers
{
    public class HomeController : ApiController
    {
        [Route("identity")]
        [Authorize]
        public IHttpActionResult Get()
        {
            var user = User as ClaimsPrincipal;
            var claims = from c in user.Claims
                         select new
                         {
                             type = c.Type,
                             value = c.Value
                         };

            return Json(claims);
        }
    }
}
