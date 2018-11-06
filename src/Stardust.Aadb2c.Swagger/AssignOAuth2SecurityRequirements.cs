using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace Stardust.Aadb2c.Swagger
{
    internal class AssignOAuth2SecurityRequirements : IOperationFilter
    {
        private readonly ScopeDescription[] _scopes;

        internal AssignOAuth2SecurityRequirements(ScopeDescription[] scopes)
        {
            _scopes = scopes;
        }

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var scopes = new List<string> { "openid" };
            scopes.AddRange(_scopes.Select(s => s.ScopeName));
            var actFilters = apiDescription.ActionDescriptor.GetFilterPipeline();
            var allowsAnonymous = actFilters.Select(f => f.Instance).OfType<OverrideAuthorizationAttribute>().Any();
            if (allowsAnonymous)
                return;
            if (operation.security == null)
                operation.security = new List<IDictionary<string, IEnumerable<string>>>();

            var oAuthRequirements = new Dictionary<string, IEnumerable<string>>
            {
                {"oauth2", scopes}
            };

            operation.security.Add(oAuthRequirements);
        }
    }
}