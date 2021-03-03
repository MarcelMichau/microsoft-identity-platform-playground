using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace SummaryApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SummaryController : ControllerBase
    {
        [HttpGet]
        [AuthorizeForScopes(Scopes = new[] { DelegatedPermissions.ReadSummary })]
        public string Get()
        {
            return "Meh";
        }
    }
}
