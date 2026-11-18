namespace Adventure;

public sealed class GameRoom
{
    private readonly Dictionary<string, int> _exits = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _objects = [];

    public GameRoom(int id, string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Id = id;
        Name = name;
        Description = description;
    }

    public int Id { get; }

    public string Name { get; }

    public string Description { get; }

    public IReadOnlyDictionary<string, int> Exits => _exits;

    public IReadOnlyList<string> Objects => _objects;

    public GameRoom AddObject(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _objects.Add(name);
        return this;
    }

    public GameRoom AddExit(string direction, int destinationRoomId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(direction);
        _exits.Add(direction, destinationRoomId);
        return this;
    }
}
