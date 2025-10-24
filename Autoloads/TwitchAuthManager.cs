using Godot;
using System.Threading.Tasks;
using TwitchBrb.Scripts;

namespace TwitchBrb.Autoloads;

public partial class TwitchAuthManager : Node
{
    public static TwitchAuthManager Instance { get; private set; }
    private const string REDIRECT_URI = "http://localhost:3000/";

    public override void _Ready()
    {
        Instance = this;
    }

    public void SaveCredentials(string clientId, string clientSecret)
        => TwitchConfigManager.Instance.AuthConfigSave(clientId, clientSecret);

    public (string ClientId, string ClientSecret) LoadCredentials() => TwitchConfigManager.Instance.AuthConfigLoadCredentials();
    public async void GetOrRefreshTokensAsync(string clientId, string clientSecret) 
        => await TwitchAuth.GetOrRefreshTokensAsync(clientId, clientSecret, REDIRECT_URI);

    public async Task<string> GetOAuthToken()
    {
        (string ClientId, string ClientSecret) = LoadCredentials();
        var tokenResponse = await TwitchAuth.GetOrRefreshTokensAsync(
            ClientId,
            ClientSecret,
            REDIRECT_URI
        );

        return $"oauth:{tokenResponse.access_token}";
    }
}
