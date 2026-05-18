using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Common;

public sealed class KeycloakAuthenticationSettings
{
    public const string SectionName = "KeycloakAuthentication";

    public KeycloakRealmAuthenticationSettings Users { get; init; } = new();

    public KeycloakRealmAuthenticationSettings System { get; init; } = new();
}

public sealed class KeycloakRealmAuthenticationSettings
{
    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string JwksUrl { get; init; } = string.Empty;
}

public static class KeycloakAuthenticationSchemes
{
    public const string Users = "KeycloakUsers";
    public const string System = "KeycloakSystem";
}

public static class KeycloakAuthentication
{
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<KeycloakAuthenticationSettings>(
            configuration.GetSection(KeycloakAuthenticationSettings.SectionName));

        services
            .AddAuthentication()
            .AddJwtBearer(KeycloakAuthenticationSchemes.Users)
            .AddJwtBearer(KeycloakAuthenticationSchemes.System);

        services
            .AddAuthorizationBuilder()
            .AddPolicy(AppAuthorizationPolicies.AuthenticatedTenant, policy =>
            {
                policy.AuthenticationSchemes.Add(KeycloakAuthenticationSchemes.Users);
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(AppClaimTypes.TenantId);
                policy.RequireAssertion(static context =>
                    !string.IsNullOrWhiteSpace(context.User.FindFirstValue(AppClaimTypes.TenantId)));
            })
            .AddPolicy(AppAuthorizationPolicies.AuthenticatedUser, policy =>
            {
                policy.AuthenticationSchemes.Add(KeycloakAuthenticationSchemes.Users);
                policy.AuthenticationSchemes.Add(KeycloakAuthenticationSchemes.System);
                policy.RequireAuthenticatedUser();
            })
            .AddPolicy(AppAuthorizationPolicies.Viewer, policy =>
            {
                policy.AuthenticationSchemes.Add(KeycloakAuthenticationSchemes.Users);
                policy.RequireAuthenticatedUser();
                policy.RequireAppRole(AppRoles.Viewer, AppRoles.Editor, AppRoles.TenantAdmin);
            })
            .AddPolicy(AppAuthorizationPolicies.Editor, policy =>
            {
                policy.AuthenticationSchemes.Add(KeycloakAuthenticationSchemes.Users);
                policy.RequireAuthenticatedUser();
                policy.RequireAppRole(AppRoles.Editor, AppRoles.TenantAdmin);
            })
            .AddPolicy(AppAuthorizationPolicies.TenantAdmin, policy =>
            {
                policy.AuthenticationSchemes.Add(KeycloakAuthenticationSchemes.Users);
                policy.RequireAuthenticatedUser();
                policy.RequireAppRole(AppRoles.TenantAdmin);
            })
            .AddPolicy(AppAuthorizationPolicies.SystemUser, policy =>
            {
                policy.AuthenticationSchemes.Add(KeycloakAuthenticationSchemes.System);
                policy.RequireAuthenticatedUser();
                policy.RequireSystemRole(
                    SystemRoles.PlatformAdmin,
                    SystemRoles.SupportAdmin,
                    SystemRoles.SecurityAdmin);
            })
            .AddPolicy(AppAuthorizationPolicies.PlatformAdmin, policy =>
            {
                policy.AuthenticationSchemes.Add(KeycloakAuthenticationSchemes.System);
                policy.RequireAuthenticatedUser();
                policy.RequireSystemRole(SystemRoles.PlatformAdmin);
            })
            .AddPolicy(AppAuthorizationPolicies.SupportAdmin, policy =>
            {
                policy.AuthenticationSchemes.Add(KeycloakAuthenticationSchemes.System);
                policy.RequireAuthenticatedUser();
                policy.RequireSystemRole(SystemRoles.SupportAdmin);
            })
            .AddPolicy(AppAuthorizationPolicies.SecurityAdmin, policy =>
            {
                policy.AuthenticationSchemes.Add(KeycloakAuthenticationSchemes.System);
                policy.RequireAuthenticatedUser();
                policy.RequireSystemRole(SystemRoles.SecurityAdmin);
            });

        services
            .AddOptions<JwtBearerOptions>(KeycloakAuthenticationSchemes.Users)
            .Configure<IOptions<KeycloakAuthenticationSettings>>((options, settings) =>
            {
                ConfigureJwtBearerOptions(options, settings.Value.Users, requireTenantId: false);
            });

        services
            .AddOptions<JwtBearerOptions>(KeycloakAuthenticationSchemes.System)
            .Configure<IOptions<KeycloakAuthenticationSettings>>((options, settings) =>
            {
                ConfigureJwtBearerOptions(options, settings.Value.System, requireTenantId: false);
            });

        return services;
    }

    public static IApplicationBuilder UseKeycloakAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static AuthorizationPolicyBuilder RequireAppRole(
        this AuthorizationPolicyBuilder policy,
        params string[] roles)
    {
        var expectedRoles = roles
            .Where(static role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (expectedRoles.Length == 0)
        {
            throw new ArgumentException("At least one role must be provided.", nameof(roles));
        }

        policy.RequireClaim(AppClaimTypes.TenantId);
        policy.RequireAssertion(context =>
        {
            var actualRoles = context.User.FindAll(AppClaimTypes.Roles).Select(static claim => claim.Value);
            return actualRoles.Intersect(expectedRoles, StringComparer.Ordinal).Any();
        });

        return policy;
    }

    public static AuthorizationPolicyBuilder RequireSystemRole(
        this AuthorizationPolicyBuilder policy,
        params string[] roles)
    {
        return policy.RequireRealmRole(roles);
    }

    private static AuthorizationPolicyBuilder RequireRealmRole(
        this AuthorizationPolicyBuilder policy,
        params string[] roles)
    {
        var expectedRoles = roles
            .Where(static role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (expectedRoles.Length == 0)
        {
            throw new ArgumentException("At least one role must be provided.", nameof(roles));
        }

        policy.RequireAssertion(context =>
        {
            var actualRoles = context.User.FindAll(AppClaimTypes.Roles).Select(static claim => claim.Value);
            return actualRoles.Intersect(expectedRoles, StringComparer.Ordinal).Any();
        });

        return policy;
    }

    private static void ConfigureJwtBearerOptions(
        JwtBearerOptions options,
        KeycloakRealmAuthenticationSettings settings,
        bool requireTenantId)
    {
        if (string.IsNullOrWhiteSpace(settings.Issuer))
        {
            throw new InvalidOperationException("Keycloak issuer must be configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.JwksUrl))
        {
            throw new InvalidOperationException("Keycloak JWKS URL must be configured.");
        }

        var validateAudience = !string.IsNullOrWhiteSpace(settings.Audience);

        options.RequireHttpsMetadata = false;
        options.MapInboundClaims = false;
        options.RefreshOnIssuerKeyNotFound = true;
        options.ConfigurationManager = new JwksConfigurationManager(settings.Issuer, settings.JwksUrl);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = validateAudience,
            ValidAudience = validateAudience ? settings.Audience : null,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            RequireExpirationTime = true,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = AppClaimTypes.Roles
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (!requireTenantId)
                {
                    return Task.CompletedTask;
                }

                var tenantId = context.Principal?.FindFirstValue(AppClaimTypes.TenantId);

                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    context.Fail("Keycloak JWT is missing tenant_id.");
                }

                return Task.CompletedTask;
            }
        };
    }
}

public static class AppAuthorizationPolicies
{
    public const string AuthenticatedTenant = "AuthenticatedTenant";
    public const string AuthenticatedUser = "AuthenticatedUser";
    public const string Viewer = "Viewer";
    public const string Editor = "Editor";
    public const string TenantAdmin = "TenantAdmin";
    public const string SystemUser = "SystemUser";
    public const string PlatformAdmin = "PlatformAdmin";
    public const string SupportAdmin = "SupportAdmin";
    public const string SecurityAdmin = "SecurityAdmin";
}

public static class AppRoles
{
    public const string Viewer = "viewer";
    public const string Editor = "editor";
    public const string TenantAdmin = "tenant-admin";
}

public static class SystemRoles
{
    public const string PlatformAdmin = "platform-admin";
    public const string SupportAdmin = "support-admin";
    public const string SecurityAdmin = "security-admin";
}

public static class AppClaimTypes
{
    public const string TenantId = "tenant_id";
    public const string Roles = "roles";
    public const string AuthzVersion = "authz_version";
}

public interface IUserContextAccessor
{
    UserContext? GetCurrent();
}

public sealed class UserContextAccessor : IUserContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserContext? GetCurrent()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return null;
        }

        var principal = httpContext.User;

        if (principal?.Identity?.IsAuthenticated != true)
        {
            return ReadFromHeaders(httpContext.Request.Headers);
        }

        var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var tenantId = principal.FindFirstValue(AppClaimTypes.TenantId);
        var authzVersionValue = principal.FindFirstValue(AppClaimTypes.AuthzVersion);
        var issuer = principal.FindFirstValue(JwtRegisteredClaimNames.Iss);
        var username = principal.FindFirstValue("preferred_username") ?? principal.Identity?.Name;
        var email = principal.FindFirstValue(JwtRegisteredClaimNames.Email);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        int? authzVersion = null;

        if (!string.IsNullOrWhiteSpace(authzVersionValue) && int.TryParse(authzVersionValue, out var parsedAuthzVersion))
        {
            authzVersion = parsedAuthzVersion;
        }

        var roles = principal.FindAll(AppClaimTypes.Roles)
            .Select(static claim => claim.Value)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return new UserContext(
            userId,
            tenantId,
            username,
            email,
            GetRealmName(issuer),
            roles,
            authzVersion);
    }

    private static UserContext? ReadFromHeaders(IHeaderDictionary headers)
    {
        var userId = headers[RequestContextHeaders.UserId].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var tenantId = headers[RequestContextHeaders.TenantId].FirstOrDefault();
        var username = headers[RequestContextHeaders.Username].FirstOrDefault();
        var email = headers[RequestContextHeaders.Email].FirstOrDefault();
        var realm = headers[RequestContextHeaders.Realm].FirstOrDefault() ?? string.Empty;
        var authzVersionValue = headers[RequestContextHeaders.AuthzVersion].FirstOrDefault();

        int? authzVersion = null;

        if (!string.IsNullOrWhiteSpace(authzVersionValue) && int.TryParse(authzVersionValue, out var parsedAuthzVersion))
        {
            authzVersion = parsedAuthzVersion;
        }

        var roles = (headers[RequestContextHeaders.Roles].FirstOrDefault() ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return new UserContext(
            userId,
            tenantId,
            username,
            email,
            realm,
            roles,
            authzVersion);
    }

    private static string GetRealmName(string? issuer)
    {
        if (string.IsNullOrWhiteSpace(issuer))
        {
            return string.Empty;
        }

        const string marker = "/realms/";
        var markerIndex = issuer.LastIndexOf(marker, StringComparison.Ordinal);

        if (markerIndex < 0)
        {
            return issuer;
        }

        var realmStartIndex = markerIndex + marker.Length;
        return realmStartIndex >= issuer.Length
            ? string.Empty
            : issuer[realmStartIndex..];
    }
}

public sealed record UserContext(
    string UserId,
    string? TenantId,
    string? Username,
    string? Email,
    string Realm,
    IReadOnlyList<string> Roles,
    int? AuthzVersion);

internal sealed class JwksConfigurationManager : IConfigurationManager<OpenIdConnectConfiguration>
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(15);
    private readonly HttpClient _httpClient = new();
    private readonly string _issuer;
    private readonly string _jwksUrl;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private OpenIdConnectConfiguration? _configuration;
    private DateTimeOffset _nextRefreshUtc = DateTimeOffset.MinValue;

    public JwksConfigurationManager(string issuer, string jwksUrl)
    {
        _issuer = issuer;
        _jwksUrl = jwksUrl;
    }

    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
    {
        if (_configuration is not null && DateTimeOffset.UtcNow < _nextRefreshUtc)
        {
            return _configuration;
        }

        await _refreshLock.WaitAsync(cancel);

        try
        {
            if (_configuration is not null && DateTimeOffset.UtcNow < _nextRefreshUtc)
            {
                return _configuration;
            }

            var json = await _httpClient.GetStringAsync(_jwksUrl, cancel);
            var keySet = new JsonWebKeySet(json);
            var configuration = new OpenIdConnectConfiguration
            {
                Issuer = _issuer,
                JsonWebKeySet = keySet
            };

            foreach (var signingKey in keySet.GetSigningKeys())
            {
                configuration.SigningKeys.Add(signingKey);
            }

            _configuration = configuration;
            _nextRefreshUtc = DateTimeOffset.UtcNow.Add(RefreshInterval);

            return configuration;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public void RequestRefresh()
    {
        _nextRefreshUtc = DateTimeOffset.MinValue;
    }
}
