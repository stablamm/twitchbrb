using Godot;
using System;
using TwitchBrb.Autoloads;

namespace TwitchBrb.Scenes.UI;

public partial class ClientIdSecretUi : Control
{
    private LineEdit _clientIdInput;
    private LineEdit _clientSecretInput;

    public override void _Ready()
    {
        _clientIdInput = GetNode<LineEdit>("%IdInput");
        _clientSecretInput = GetNode<LineEdit>("%SecretInput");

        var (clientId, clientSecret) = TwitchAuthManager.Instance.LoadCredentials();
        _clientIdInput.Text = clientId;
        _clientSecretInput.Text = clientSecret;
    }

    public void OnSaveButtonPressed()
    {
        if (string.IsNullOrEmpty(_clientIdInput.Text) || string.IsNullOrEmpty(_clientSecretInput.Text))
        {
            GD.PrintErr("Client ID and Client Secret cannot be empty.");
            return;
        }

        TwitchAuthManager.Instance.SaveCredentials(_clientIdInput.Text, _clientSecretInput.Text);
    }
}
