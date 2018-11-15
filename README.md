# Stardust.Aadb2c.AuthenticationFilter
Simple Authentication Filter for WebApi that supports Azure AD B2C

## Usage

### Install nuget package
```nuget
PM>  Install-Package Stardust.Aadb2c.AuthenticationFilter -Version 2.0.0-pre0004
```
### .net Framework
#### add filter
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

#### Configure filter

##### In config
```XML
<appSettings>
    <!-- mandatory -->
    <add key ="aadTenant" value="tenantName.onmicrosoft.com" />
    <add key="audience" value="your appid" /><!-- supports ; separated list of audiences.-->
    <add key="issuerHostName" value="https://login.microsoftonline.com/{your tenant id}/v2.0/" />
    <add key="audience" value="you appid" /><!-- supports ; separated list of audiences.-->
    <add key="issuerHostName_v1" value="https://sts.windows.net/{your tenant id}/" />
    <add key="audience_v1" value="your V1 appid" />
    <!-- optional -->
    <add key ="aadPolicy" value="B2C_1A_SignIn" />
</appSettings>
```

##### In code
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
### asp.net core
#### add filter
```CS
public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddB2CAuthentication("OAuth2", "Azure B2C authentication");//Add the B2C authentication scheme
        }

         public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
            app.AddConfigurationManager(new ConfigManager());// Add the configuration binding. Implement your own manager to fit with your configuration scheme.
            //the netcore version uses the same config keys as the .net framework version.
            app.UseMvc();            
        }
```
# Swagger UI support for OAuth2 implicit grant flow

## Usage

### Install nuget package
```nuget
PM>  Install-Package Swashbuckle
PM>  Install-Package Stardust.Aadb2c.Swagger
```
> note: Install the Swashbuckle package first, this ensures that the swaggerconfig is crated properly

### enable oauth support

In App_Start/SwaggerConfig.cs add the following

```CS
    GlobalConfiguration.Configuration
        .EnableSwagger(c =>
            {
                c.EnableAzureAdB2cOAuth2(
                                            tenantId, true,
                                            new ScopeDescription
                                            {
                                                Description = "Allow the service to act on behalf of the user",
                                                ScopeName =  scopeName //usually in the format: https://tenantName.onmicrosoft.com/appId/scopeName (https://stardustfx123.onmicrosoft.com/739B91C4-26A7-4D6C-9344-5FF77A87C09A/user_impersonation)
                                            });
            }).EnableSwaggerUi(c =>
                {
                    c.EnableAzureAdB2cOAuth2(swaggerUiClientId, "B2C_1A_SignIn");
                    
                }); 
```

#### alternative
you can keep all the parameters passed to the swagger in the config file.


```CS
    GlobalConfiguration.Configuration
        .EnableSwagger(c =>
            {
                c.EnableAzureAdB2cOAuth2();
            }).EnableSwaggerUi(c =>
                {
                    c.EnableAzureAdB2cOAuth2();
                    
                }); 
```

```XML
<appSettings>
    <!-- mandatory -->
    <add key ="aadTenantId" value="tenantId" /><!-- this is a guid -->
    <!-- scopes are separated by | and name and description is separated by ; -->
    <add key="aadScopes" value="email;send email|https://stardustfx123.onmicrosoft.com/739B91C4-26A7-4D6C-9344-5FF77A87C09A/user_impersonation;Allow the service to act on behalf of the user" />
    <add key="aadFlowDescription" value="OAuth2 Implicit Grant" />
    <add key ="aadPolicy" value="B2C_1A_SignIn" />
    <add key ="aadUseV2Endpoint" value="true" />
    <add key ="swaggerClientId" value="swaggerAppId" /><!-- this is a guid -->
    <add key ="swaggerClientSecret" value="secret" /><!-- not recommended to use this -->
    <add key ="swaggerAppName" value="Swagger UI" />
</appSettings>

```