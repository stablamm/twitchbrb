using Godot;
using TwitchBrb.Autoloads;

namespace TwitchBrb.Scenes.UI;

public partial class JoinButton : Button
{
    public async void OnJoinButtonPress()
    {
        (string username, string channel) = TwitchConfigManager.Instance.ChatManagerConfigLoad();
        var oauthToken = await TwitchAuthManager.Instance.GetOAuthToken();
        TwitchChatManager.Instance.StartChatListener(
            username,
            channel,
            oauthToken
        );
    }
}
