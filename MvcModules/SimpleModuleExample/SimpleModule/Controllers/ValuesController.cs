using System;
using System.Drawing;
using System.IO;
using System.Web.Http;

namespace SimpleModule.Controllers
{
    public class ValuesController : ApiController
    {
        public IHttpActionResult Get()
        {
            var bd = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(bd, "untitled.png");
            var image = Image.FromFile(path);
            image.Save(Path.Combine(bd, "test.png"));
            return Ok(new string[] { "Hello", "World" });
        }
    }
}
