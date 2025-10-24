using Godot;
using TwitchBrb.Autoloads;

namespace TwitchBrb.Scenes.UI;

public partial class TwitchChannelConnectorUi : Control
{
    private LineEdit _username;
    private LineEdit _channel;

    public override void _Ready()
    {
        _username = GetNode<LineEdit>("%Username");
        _channel = GetNode<LineEdit>("%Channel");

        LoadConfig();
    }

    public void OnSaveButtonPressed()
    {
        var username = _username.Text.Trim();
        var channel = _channel.Text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(channel))
        {
            //TODO: Notify visually via UI.
            GD.PrintErr("Username and Channel cannot be empty.");
            return;
        }

        TwitchConfigManager.Instance.ChatManagerConfigSave(username, channel);
    }

    public void LoadConfig()
    {
        (string username, string channel) = TwitchConfigManager.Instance.ChatManagerConfigLoad();
        
        _username.Text = username;
        _channel.Text = channel;
    }
}
