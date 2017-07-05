# Stardust.Aadb2c.AuthenticationFilter
Simple Authentication Filter for WebApi that supports Azure AD B2C

## Usage

### Install nuget package
```nuget
PM>  Install-Package Stardust.Aadb2c.AuthenticationFilter 
```

### add filter
In WebApiConfig.cs add 
```CS
public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            //   config.SuppressDefaultHostAuthentication();
            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Filters.Add(new OAuthAuthenticationFilter());// turns on aad b2c token validation
            config.Filters.Add(new ErrorFilter());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
```

### Configure filter

#### In config
```XML
<appSettings>
    <!-- mandatory -->
    <add key ="aadTenant" value="tenantName.onmicrosoft.com" />
    <add key="audience" value="you appid" />
    <add key="issuerHostName" value="https://login.microsoftonline.com/{your tenant id}/v2.0/" />
    <!-- optional -->
    <add key ="aadPolicy" value="B2C_1A_SignIn" />
</appSettings>
```

#### In code
```CS
    protected void Application_Start()
    {
        //Mandatory
        B2CGlobalConfiguration.AadTenant="tenantName.onmicrosoft.com";
        B2CGlobalConfiguration.Audience="you appid";
        B2CGlobalConfiguration.ValidIssuer="https://login.microsoftonline.com/{your tenant id}/v2.0/";
        //Optional
        B2CGlobalConfiguration.AadPolicy="B2C_1A_SignIn"

        //Regular app start stuff
        AreaRegistration.RegisterAllAreas();
        GlobalConfiguration.Configure(WebApiConfig.Register);
        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        RouteConfig.RegisterRoutes(RouteTable.Routes);
        BundleConfig.RegisterBundles(BundleTable.Bundles);
    }
```