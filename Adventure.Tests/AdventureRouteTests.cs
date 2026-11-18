using System.Text.Json;
using Adventure;

namespace Adventure.Tests;

[TestClass]
public sealed class AdventureRouteTests
{
    [TestMethod]
    public void EveryConfiguredExitNavigatesToItsDestination()
    {
        var mapPath = Path.Combine(AppContext.BaseDirectory, "Adventure.json");
        using var document = JsonDocument.Parse(File.ReadAllText(mapPath));
        var map = document.RootElement;
        var rooms = map.GetProperty("rooms").EnumerateArray()
            .Select(room => new Room(
                room.GetProperty("id").GetInt32(),
                room.GetProperty("name").GetString()!,
                room.GetProperty("description").GetString()!,
                room.GetProperty("exits").EnumerateObject()
                    .ToDictionary(exit => exit.Name, exit => exit.Value.GetInt32())))
            .ToDictionary(room => room.Id);
        var startRoomName = map.GetProperty("startRoom").GetString()!;
        var startRoomId = rooms.Values.Single(room => room.Name == startRoomName).Id;
        var checkedRoutes = 0;

        foreach (var room in rooms.Values)
        {
            foreach (var exit in room.Exits)
            {
                var commands = FindRoute(startRoomId, room.Id, rooms)
                    .Select(direction => $"go {direction}")
                    .ToList();
                commands.Add($"go {exit.Key}");
                commands.Add("quit");

                var game = AdventureMapLoader.Load(mapPath);
                using var input = new StringReader(string.Join(Environment.NewLine, commands));
                using var output = new StringWriter();
                game.Run(input, output);

                var expectedRoom = rooms[exit.Value];
                var expectedRoomDescription = string.Join(
                    Environment.NewLine,
                    string.Empty,
                    expectedRoom.Name,
                    expectedRoom.Description,
                    $"Exits: {string.Join(", ", expectedRoom.Exits.Keys)}");
                Assert.IsTrue(
                    output.ToString().Contains(expectedRoomDescription, StringComparison.Ordinal),
                    $"Route from '{room.Name}' via '{exit.Key}' did not reach '{expectedRoom.Name}'. Output: {output}");
                checkedRoutes++;
            }
        }

        Assert.IsTrue(checkedRoutes > 0, "The adventure map did not contain any exits to test.");
    }

    private static List<string> FindRoute(int startRoomId, int targetRoomId, IReadOnlyDictionary<int, Room> rooms)
    {
        var pending = new Queue<(int RoomId, List<string> Directions)>();
        var visited = new HashSet<int> { startRoomId };
        pending.Enqueue((startRoomId, []));

        while (pending.TryDequeue(out var current))
        {
            if (current.RoomId == targetRoomId)
            {
                return current.Directions;
            }

            foreach (var exit in rooms[current.RoomId].Exits)
            {
                if (!visited.Add(exit.Value))
                {
                    continue;
                }

                var directions = new List<string>(current.Directions) { exit.Key };
                pending.Enqueue((exit.Value, directions));
            }
        }

        Assert.Fail($"Room ID '{targetRoomId}' is not reachable from start room ID '{startRoomId}'.");
        return [];
    }

    private sealed record Room(int Id, string Name, string Description, Dictionary<string, int> Exits);
}
