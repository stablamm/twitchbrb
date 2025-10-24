using Godot;
using System;
using System.Text;

namespace TwitchBrb.Autoloads;

public static class TwitchUtilities
{
    public static string Encode(string plainText)
    {
        var bytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(bytes);
    }

    public static string Decode(string encodedText)
    {
        try
        {
            var bytes = Convert.FromBase64String(encodedText);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            GD.PrintErr("Failed to decode credentials. File may be corrupted.");
            return string.Empty;
        }
    }
}
