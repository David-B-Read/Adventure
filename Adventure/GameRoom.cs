namespace Adventure;

public sealed class GameRoom
{
    private readonly Dictionary<string, string> _exits = new(StringComparer.OrdinalIgnoreCase);

    public GameRoom(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Name = name;
        Description = description;
    }

    public string Name { get; }

    public string Description { get; }

    public IReadOnlyDictionary<string, string> Exits => _exits;

    public GameRoom AddExit(string direction, string destinationRoomName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(direction);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationRoomName);
        _exits.Add(direction, destinationRoomName);
        return this;
    }
}
