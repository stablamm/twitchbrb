using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TwitchBrb.Autoloads;
using HttpClient = System.Net.Http.HttpClient;

namespace TwitchBrb.Scripts;

public static class TwitchAuth
{
    public static async Task<TokenResponse> GetOrRefreshTokensAsync(string clientId, string clientSecret, string redirectUri)
    {
        if (!File.Exists(TwitchConfigManager.Instance.TokenFile))
        {
            GD.Print("⚙️ Token file not found — starting new authorization...");
            return await GetInitialTokensAsync(clientId, clientSecret, redirectUri);
        }

        var saved = JsonSerializer.Deserialize<TokenResponse>(File.ReadAllText(TwitchConfigManager.Instance.TokenFile));
        var age = DateTime.UtcNow - saved.issued_at;

        if (age.TotalSeconds < saved.expires_in - 60)
        {
            GD.Print("✅ Using cached access token.");
            return saved;
        }

        GD.Print("🔁 Token expired — refreshing...");
        return await RefreshTokenAsync(clientId, clientSecret, saved.refresh_token);
    }

    private static async Task<TokenResponse> GetInitialTokensAsync(string clientId, string clientSecret, string redirectUri)
    {
        string scopes = "chat:read chat:edit";
        string authUrl =
            $"https://id.twitch.tv/oauth2/authorize?response_type=code&client_id={clientId}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scopes)}";

        // Launch browser to authorize
        OS.ShellOpen(authUrl);

        using var listener = new HttpListener();
        listener.Prefixes.Add(redirectUri);
        listener.Start();
        GD.Print("🌐 Waiting for Twitch authorization...");

        var context = await listener.GetContextAsync();
        string code = context.Request.QueryString["code"];

        string html = "<html><body><h2>✅ You can close this window.</h2></body></html>";
        byte[] buffer = Encoding.UTF8.GetBytes(html);
        await context.Response.OutputStream.WriteAsync(buffer);
        context.Response.OutputStream.Close();
        listener.Stop();

        var tokens = await ExchangeCodeForTokens(clientId, clientSecret, redirectUri, code);
        TwitchConfigManager.Instance.TokenConfigSave(tokens);
        GD.Print("✅ Received and saved new tokens.");
        return tokens;
    }

    private static async Task<TokenResponse> ExchangeCodeForTokens(string clientId, string clientSecret, string redirectUri, string code)
    {
        using var http = new HttpClient();
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("redirect_uri", redirectUri)
        });

        var response = await http.PostAsync("https://id.twitch.tv/oauth2/token", content);
        string json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Token request failed: {response.StatusCode}\n{json}");

        var tokens = JsonSerializer.Deserialize<TokenResponse>(json);
        tokens.issued_at = DateTime.UtcNow;
        TwitchConfigManager.Instance.TokenConfigSave(tokens);
        return tokens;
    }

    private static async Task<TokenResponse> RefreshTokenAsync(string clientId, string clientSecret, string refreshToken)
    {
        using var http = new HttpClient();
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret)
        });

        var response = await http.PostAsync("https://id.twitch.tv/oauth2/token", content);
        string json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Refresh failed: {response.StatusCode}\n{json}");

        var tokens = JsonSerializer.Deserialize<TokenResponse>(json);
        tokens.issued_at = DateTime.UtcNow;
        TwitchConfigManager.Instance.TokenConfigSave(tokens);
        GD.Print("✅ Token refreshed and saved.");
        return tokens;
    }
}

public class TokenResponse
{
    public string access_token { get; set; }
    public string refresh_token { get; set; }
    public int expires_in { get; set; }
    public List<string> scope { get; set; }
    public string token_type { get; set; }
    public DateTime issued_at { get; set; }
}