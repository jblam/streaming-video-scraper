using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NetflixScrape.Server.Controllers
{
    [Produces("application/json")]
    [Route("api/Status")]
    public class StatusController : Controller
    {
        public struct StatusItem
        {
            public StatusItem(string level, string status)
            {
                Level = level;
                Status = status;
            }
            public string Level { get; }
            public string Status { get; }
        }

        // GET: api/Status
        [HttpGet]
        public IEnumerable<StatusItem> Get()
        {
            return new[]
            {
                new StatusItem("Server", "OK")
            };
        }
    }
}
