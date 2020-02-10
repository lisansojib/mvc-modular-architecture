using System.Web.Http;

namespace SimpleModule.Controllers
{
    public class ValuesController : ApiController
    {
        public IHttpActionResult Get()
        {
            return Ok(new string[] { "Hello", "World" });
        }
    }
}
