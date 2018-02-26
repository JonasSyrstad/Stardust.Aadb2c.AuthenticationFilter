using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Common;
using Stardust.Interstellar.Rest.Extensions;

namespace NetCoreTestWeb.Controllers
{
    [IRoutePrefix("api/test")]
    [AuthorizeWrapper]
    [ServiceInformation]
    public interface IMyServies
    {
        [Get]
        [IRoute("Echo/{value}")]
        string Echo([In(InclutionTypes.Path)] string value);

        [Get]
        [IRoute("Echo2/{value}")]
        Task<StringWrapper> Echo2Async([In(InclutionTypes.Path)] string value);

    }

    public class ServiceInformationAttribute : HeaderInspectorAttributeBase, IHeaderHandler
    {
        public override IHeaderHandler[] GetHandlers()
        {
            return new[] { this };
        }

        public void SetHeader(HttpWebRequest req)
        {

        }

        public void GetHeader(HttpWebResponse response)
        {
        }

        public void GetServiceHeader(HttpRequestHeaders headers)
        {

        }

        public void GetServiceHeader(IDictionary<string, StringValues> headers)
        {
        }

        public void SetServiceHeaders(HttpResponseHeaders headers)
        {
        }

        public void SetServiceHeaders(IDictionary<string, StringValues> headers)
        {
            headers.Add("x-app-type", "netcore");
        }

        public int ProcessingOrder { get { return 0; } }
    }

    public class StringWrapper
    {
        public string Value { get; set; }
        public string User { get; set; }
    }

    class MyServies : IMyServies
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;

        public MyServies(IHttpContextAccessor httpContextAccessor, ILogger logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        public string Echo(string value)
        {
            return value;
        }

        public Task<StringWrapper> Echo2Async(string value)
        {
            _logger?.Message(string.Join("\n", _httpContextAccessor.HttpContext.User.Claims.Select(c => $"{c.Type} - {c.Value}")));
            return Task.FromResult(new StringWrapper { Value = value, User = _httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value });
        }
    }

    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet]
        [Route("get{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
