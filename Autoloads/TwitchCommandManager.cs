using Godot;
using System;
using System.Linq;

namespace TwitchBrb.Autoloads;

public partial class TwitchCommandManager : Node
{
    public enum COMMAND_TYPE
    {
        UNKNOWN,
        PLANT,
        WATER
    }

    public static TwitchCommandManager Instance;

    private int _ticksBeforeAction= 2;
    private int _currentTick = 0;

    public override void _Ready()
    {
        Instance = this;

        SignalManager.Instance.Connect(
            SignalManager.SignalName.ChatMessageReceived,
            new Callable(this, nameof(OnChatCommandReceived))
        );

        SignalManager.Instance.Connect(
            SignalManager.SignalName.OnTick,
            new Callable(this, nameof(OnTick))
        );
    }

    public void OnChatCommandReceived(string user, string message)
    {
        if (!message.StartsWith("!")) return; // All commands start with "!"
        if (!IsValidCommand(message)) return; // Not a valid command

        QueueManager.Instance.EnqueueCommand(message);
    }

    public T ParseCommand<T>(COMMAND_TYPE commandType, string command)
    {
        object result = commandType switch
        {
            COMMAND_TYPE.PLANT => ParsePlantCommand(command),
            COMMAND_TYPE.WATER => ParseWaterCommand(command),
            _ => throw new ArgumentException($"Unknown command type: {commandType}"),
        };

        if (result is T castResult)
        {
            return castResult;
        }

        throw new InvalidCastException(
            $"Parsed command returns {result.GetType().Name}, not expected {typeof(T).Name}");
    }

    public COMMAND_TYPE GetCommandType(string command)
    {
        if (command.StartsWith("!plant"))
            return COMMAND_TYPE.PLANT;
        else if (command.StartsWith("!water"))
            return COMMAND_TYPE.WATER;
        else
            return COMMAND_TYPE.UNKNOWN;
    }

    private bool IsValidCommand(string command)
        => command.StartsWith("!plant") 
            || command.StartsWith("!water");

    private void OnTick()
    {
        if (StateManager.Instance.IsFarmerBusy)
            return;

        _currentTick++;
        if (_currentTick < _ticksBeforeAction)
            return;

        //var command = QueueManager.Instance.DequeueCommand();
        //if (string.IsNullOrEmpty(command))
        //    return;

        SignalManager.Instance.EmitRunNextCommand();
        //var commandType = GetCommandType(command);
        //if (commandType == COMMAND_TYPE.PLANT)
        //{
        //    SignalManager.Instance.EmitPlantSeedCommand(command);
        //}
        //else if (commandType == COMMAND_TYPE.WATER)
        //{
        //    SignalManager.Instance.EmitWaterCropCommand(command);
        //}

        _currentTick = 0;
    }

    private Vector2 ParsePlantCommand(string command)
        => ParseCoordinates(command);

    private Vector2 ParseWaterCommand(string command)
        => ParseCoordinates(command);

    private Vector2 ParseCoordinates(string command)
    {
        // Example commands for Location (2, 2):
        //  !<command> A 1
        //  !<command> 1 A
        //  !<command> A1
        //  !<command> 1A
        var parts = command.Split(' ');
        if (parts.Length >= 2)
        {
            string data = string.Join("", parts.Skip(1));

            string letter = "";
            string number = "";

            // Extract letter and number regardless of order
            foreach (char c in data)
            {
                if (char.IsLetter(c))
                    letter += c;
                else if (char.IsDigit(c))
                    number += c;
            }

            if (string.IsNullOrEmpty(letter) || string.IsNullOrEmpty(number))
                return Vector2.Zero;

            // Convert to coordinates using your mapping methods
            MapLetterToCoordinate(letter, out int x);
            MapNumberToCoordinate(int.Parse(number), out int y);
            
            var outputPos = new Vector2(x, y);

            if (outputPos.X > 16 || outputPos.Y > 10) // 16x10 grid limit
                return Vector2.Zero; // Out of bounds

            return new Vector2(x, y);
        }
        return Vector2.Zero;
    }

    private void MapLetterToCoordinate(string letter, out int coordinate)
    {
        coordinate = -1;
        letter = letter.ToUpper();
        if (letter.Length == 1 && letter[0] >= 'A' && letter[0] <= 'O')
        {
            coordinate = letter[0] - 'A' + 2;
        }
    }

    private void MapNumberToCoordinate(int number, out int coordinate) 
        => coordinate = number + 1;
}