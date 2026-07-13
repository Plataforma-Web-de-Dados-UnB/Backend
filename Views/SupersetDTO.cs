using System.Text.Json.Serialization;

namespace api.Views;

public class SupersetGuestTokenRequestDto
{
    public string DashboardId { get; set; } = string.Empty;
}

public class SupersetGuestTokenResponseDto
{
    public string Token { get; set; } = string.Empty;
}

public class SupersetGuestTokenPayload
{
    public SupersetGuestTokenUser User { get; set; } = new();
    public List<SupersetGuestTokenResource> Resources { get; set; } = new();
    public List<object> Rls { get; set; } = new();
}

public class SupersetGuestTokenUser
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;
}

public class SupersetGuestTokenResource
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "dashboard";

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public class SupersetLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Provider { get; set; } = "db";
    public bool Refresh { get; set; } = true;
}

public class SupersetLoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
}

public class SupersetSsoUrlResponseDto
{
    public string Url { get; set; } = string.Empty;
}
