using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace summary_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SummaryController : ControllerBase
    {
        [HttpGet]
        [AuthorizeForScopes(Scopes = new[] { "Summary.Read" })]
        public string Get()
        {
            return "Meh";
        }
    }
}
