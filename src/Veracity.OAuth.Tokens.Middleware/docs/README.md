# Veracity.OAuth.Tokens.Middleware

Authentication middleware for Veracity services that supports both **Azure AD B2C** and **Auth0 (IdP V2)** token validation in a single handler.

## Migration from Stardust.Aadb2c.AuthenticationFilter

If you are currently using `Stardust.Aadb2c.AuthenticationFilter` with `AddB2CAuthentication`, migrating to this package adds Auth0 support while keeping your existing B2C configuration unchanged.

### Step 1 — Replace the NuGet package

Remove:
```
Stardust.Aadb2c.AuthenticationFilter
```

Install:
```
Veracity.OAuth.Tokens.Middleware
```

### Step 2 — Update authentication registration

**Before** (B2C only):
```csharp
using Stardust.Aadb2c.AuthenticationFilter;

builder.Services.AddAuthentication("OAuth2")
	.AddB2CAuthentication();
```

**After** (B2C + Auth0):
```csharp
using Stardust.Aadb2c.AuthenticationFilter.Core;
using Veracity.OAuth.Tokens.Middleware;

// B2C configuration — unchanged from before
B2CGlobalConfiguration.Audience = "your-b2c-audience";
B2CGlobalConfiguration.ValidIssuer = "https://login.microsoftonline.com/.../v2.0";
B2CGlobalConfiguration.B2cTenantUrl = "https://yourtenant.b2clogin.com/yourtenant.onmicrosoft.com";

// Auth0 / IdP V2 configuration — new
Auth0Configuration.Domain = "your-tenant.auth0.com";
Auth0Configuration.Audience = "https://your-api-audience";
// Auth0Configuration.ValidIssuer is optional; defaults to "https://{Domain}/"

// Register the middleware
builder.Services.AddAuthentication(VeracityAuthenticationExtensions.DefaultScheme)
	.AddVeracityAuthentication();
```

### Step 3 — Remove old using statements

Replace any references to `Stardust.Aadb2c.AuthenticationFilter` namespace with `Veracity.OAuth.Tokens.Middleware` for the authentication handler registration. The `Stardust.Aadb2c.AuthenticationFilter.Core` namespace is still used for `B2CGlobalConfiguration`.

## Configuration keys

All configuration uses the `ConfigurationManagerHelper` pattern.

### B2C keys (inherited from Stardust)

| Key | Description |
|-----|-------------|
| `audience` | Semicolon-separated list of valid B2C audiences |
| `aadTenant` | Azure AD tenant name |
| `aadPolicy` | B2C policy name |
| `issuerHostName` | Valid B2C issuer URL |
| `b2cTenantUrl` | B2C tenant base URL |

### Auth0 / IdP V2 keys (new)

| Key | Description |
|-----|-------------|
| `IdPV2Domain` | Auth0 domain (e.g., `my-tenant.auth0.com`) |
| `IdPV2Audience` | Semicolon-separated list of valid Auth0 audiences |
| `IdPV2Issuer` | Valid issuer URL (optional, defaults to `https://{Domain}/`) |

## How it works

1. The middleware extracts the bearer token from the `Authorization` header.
2. The token's `iss` (issuer) claim is inspected to determine the token type.
3. If the issuer host matches the configured Auth0 domain, the token is validated against Auth0's OIDC discovery endpoint.
4. Otherwise, the token is validated using the existing B2C/AAD validator from `Stardust.Aadb2c.AuthenticationFilter.Core`.
5. The validated `ClaimsPrincipal` is set on `HttpContext.User`.

## Security

- All signing keys are fetched over HTTPS from the identity provider's OIDC discovery endpoint.
- Auth0 tokens are restricted to RS256 signing algorithm.
- Token lifetime, audience, issuer, and signature are all validated.
- Unsigned tokens (`alg: none`) are rejected.
- Error responses do not leak internal details.

## B2C-only mode

If no Auth0 domain is configured (`IdPV2Domain` is not set), the middleware operates in B2C-only mode, behaving identically to the original `AddB2CAuthentication` handler.
