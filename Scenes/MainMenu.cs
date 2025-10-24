using Godot;
using TwitchBrb.Autoloads;

public partial class MainMenu : Node2D
{
    public async void OnStartButtonPressed()
    {
        (string username, string channel) = TwitchConfigManager.Instance.ChatManagerConfigLoad();
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(channel))
        {
            GD.PrintErr("⚠️ Twitch username or channel is not set in the configuration.");
            GetTree().ChangeSceneToFile("res://Scenes/Menus/settings_menu.tscn");
            return;
        }
        var oauthToken = await TwitchAuthManager.Instance.GetOAuthToken();
        TwitchChatManager.Instance.StartChatListener(
            username,
            channel,
            oauthToken
        );

        GetTree().ChangeSceneToFile("res://Scenes/brb_screen.tscn");
    }

    public void OnSettingsButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Menus/settings_menu.tscn");
    }
}
