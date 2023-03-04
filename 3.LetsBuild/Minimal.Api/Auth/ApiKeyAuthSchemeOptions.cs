using Microsoft.AspNetCore.Authentication;

namespace Minimal.Api.Auth;

public class ApiKeyAuthSchemeOptions: AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = "VerySecret";
}