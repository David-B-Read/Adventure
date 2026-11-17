using System.Text.Json;

namespace Adventure;

public static class AdventureMapLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static AdventureGame Load(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var json = File.ReadAllText(filePath);
        AdventureMapData map;
        try
        {
            map = JsonSerializer.Deserialize<AdventureMapData>(json, SerializerOptions)
                ?? throw new InvalidDataException("The adventure map must contain a JSON object.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"Adventure map '{filePath}' contains invalid JSON.", exception);
        }

        var title = RequireValue(map.Title, "title");
        var introduction = RequireValue(map.Introduction, "introduction");
        var startRoom = RequireValue(map.StartRoom, "startRoom");
        if (map.Rooms is null || map.Rooms.Count == 0)
        {
            throw new InvalidDataException("The adventure map must define at least one room.");
        }

        var rooms = new List<GameRoom>(map.Rooms.Count);
        for (var index = 0; index < map.Rooms.Count; index++)
        {
            var roomData = map.Rooms[index]
                ?? throw new InvalidDataException($"Room at index {index} must be a JSON object.");
            var roomId = roomData.Id
                ?? throw new InvalidDataException($"Adventure map property 'rooms[{index}].id' is required.");
            var roomName = RequireValue(roomData.Name, $"rooms[{index}].name");
            var description = RequireValue(roomData.Description, $"rooms[{index}].description");
            var room = new GameRoom(roomId, roomName, description);

            if (roomData.Exits is null)
            {
                throw new InvalidDataException($"Room '{roomName}' must have an exits object (use {{}} for no exits).");
            }

            foreach (var (direction, destination) in roomData.Exits)
            {
                room.AddExit(direction, destination);
            }

            rooms.Add(room);
        }

        return new AdventureGame(title, introduction, startRoom, rooms);
    }

    private static string RequireValue(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException($"Adventure map property '{propertyName}' is required and cannot be empty.");
        }

        return value;
    }

    private sealed class AdventureMapData
    {
        public string? Title { get; set; }
        public string? Introduction { get; set; }
        public string? StartRoom { get; set; }
        public List<AdventureRoomData?>? Rooms { get; set; }
    }

    private sealed class AdventureRoomData
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, int>? Exits { get; set; } = new();
    }
}
