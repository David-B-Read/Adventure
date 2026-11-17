namespace Adventure;

public sealed class AdventureGame
{
    private readonly string _title;
    private readonly string _introduction;
    private readonly Dictionary<int, GameRoom> _rooms = new();
    private readonly Dictionary<string, (string HelpText, Action<GameContext, string> Handler)> _commands =
        new(StringComparer.OrdinalIgnoreCase);
    private GameRoom _currentRoom;
    private TextWriter _output = Console.Out;
    private bool _isRunning;

    public AdventureGame(string title, string introduction, string startRoomName, IEnumerable<GameRoom> rooms)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(introduction);
        ArgumentException.ThrowIfNullOrWhiteSpace(startRoomName);
        ArgumentNullException.ThrowIfNull(rooms);

        _title = title;
        _introduction = introduction;

        foreach (var room in rooms)
        {
            ArgumentNullException.ThrowIfNull(room);
            if (!_rooms.TryAdd(room.Id, room))
            {
                throw new ArgumentException($"Room ID '{room.Id}' is duplicated.", nameof(rooms));
            }
        }

        _currentRoom = _rooms.Values.FirstOrDefault(room =>
            string.Equals(room.Name, startRoomName, StringComparison.OrdinalIgnoreCase))!;
        if (_currentRoom is null)
        {
            throw new ArgumentException($"Start room '{startRoomName}' was not found.", nameof(startRoomName));
        }

        foreach (var room in _rooms.Values)
        {
            foreach (var (direction, destination) in room.Exits)
            {
                if (!_rooms.ContainsKey(destination))
                {
                    throw new ArgumentException(
                        $"Room '{room.Name}' has an exit '{direction}' to unknown room ID '{destination}'.",
                        nameof(rooms));
                }
            }
        }

        RegisterCommand("help", "Show available commands.", (_, _) => ShowHelp());
        RegisterCommand("look", "Describe your current room.", (_, _) => DescribeCurrentRoom());
        RegisterCommand("go", "Move through an exit (for example: go north).", Go);
        RegisterCommand("quit", "Leave the game.", (context, _) => context.Stop());
        RegisterCommand("exit", "Leave the game.", (context, _) => context.Stop());
    }

    public void RegisterCommand(string name, string helpText, Action<GameContext, string> handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(helpText);
        ArgumentNullException.ThrowIfNull(handler);

        if (name.Any(char.IsWhiteSpace))
        {
            throw new ArgumentException("Command names cannot contain whitespace.", nameof(name));
        }

        _commands.Add(name, (helpText, handler));
    }

    public void Run(TextReader? input = null, TextWriter? output = null)
    {
        input ??= Console.In;
        _output = output ?? Console.Out;
        _isRunning = true;

        _output.WriteLine(_title);
        _output.WriteLine(_introduction);
        DescribeCurrentRoom();

        while (_isRunning)
        {
            _output.Write("> ");
            var line = input.ReadLine();
            if (line is null)
            {
                break;
            }

            ProcessCommand(line);
        }
    }

    private void ProcessCommand(string line)
    {
        var parts = line.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return;
        }

        var commandName = parts[0];
        var arguments = parts.Length > 1 ? parts[1] : string.Empty;
        if (_commands.TryGetValue(commandName, out var command))
        {
            command.Handler(new GameContext(this), arguments);
        }
        else
        {
            _output.WriteLine($"I don't know that command. Type 'help' to see what you can do.");
        }
    }

    private void ShowHelp()
    {
        _output.WriteLine("Commands:");
        foreach (var (name, command) in _commands.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            _output.WriteLine($"  {name} - {command.HelpText}");
        }
    }

    private void DescribeCurrentRoom()
    {
        _output.WriteLine();
        _output.WriteLine(_currentRoom.Name);
        _output.WriteLine(_currentRoom.Description);
        _output.WriteLine(_currentRoom.Exits.Count == 0
            ? "There are no visible exits."
            : $"Exits: {string.Join(", ", _currentRoom.Exits.Keys)}");
    }

    private void Go(GameContext context, string direction)
    {
        if (string.IsNullOrWhiteSpace(direction))
        {
            _output.WriteLine("Go where? Try 'go north'.");
            return;
        }

        if (!_currentRoom.Exits.TryGetValue(direction.Trim(), out var destination))
        {
            _output.WriteLine($"There is no exit '{direction.Trim()}' from here.");
            return;
        }

        context.MoveTo(destination);
    }

    internal GameRoom CurrentRoom => _currentRoom;

    internal void MoveTo(int roomId)
    {
        if (!_rooms.TryGetValue(roomId, out var room))
        {
            throw new ArgumentException($"Room ID '{roomId}' was not found.", nameof(roomId));
        }

        _currentRoom = room;
        DescribeCurrentRoom();
    }

    internal void Stop() => _isRunning = false;

    internal TextWriter Output => _output;
}

public sealed class GameContext
{
    private readonly AdventureGame _game;

    internal GameContext(AdventureGame game) => _game = game;

    public GameRoom CurrentRoom => _game.CurrentRoom;

    public TextWriter Output => _game.Output;

    public void MoveTo(int roomId) => _game.MoveTo(roomId);

    public void Stop() => _game.Stop();
}
