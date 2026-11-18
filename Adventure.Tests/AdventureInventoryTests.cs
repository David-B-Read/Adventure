using Adventure;

namespace Adventure.Tests;

[TestClass]
public sealed class AdventureInventoryTests
{
    [TestMethod]
    public void PlayerCanTakeCarryLeaveAndTakeObjectInAnotherRoom()
    {
        var output = RunGame(
            "go west",
            "go north",
            "look",
            "take brass key",
            "inventory",
            "go south",
            "leave brass key",
            "inventory",
            "look",
            "take brass key",
            "inventory",
            "look",
            "quit");

        StringAssert.Contains(output, "Objects: brass key, pocket watch");
        StringAssert.Contains(output, "You take the brass key.");
        StringAssert.Contains(output, "You are carrying: brass key");
        StringAssert.Contains(output, "You leave the brass key here.");
        StringAssert.Contains(output, "You are carrying nothing.");
        StringAssert.Contains(output, "Objects: brass key, silver candlestick");
        StringAssert.Contains(output, "Objects: silver candlestick");
    }

    [TestMethod]
    public void PlayerCannotTakeUnavailableObjectOrLeaveUncarriedObject()
    {
        var output = RunGame("take brass key", "leave brass key", "inventory", "quit");

        StringAssert.Contains(output, "There is no 'brass key' here to take.");
        StringAssert.Contains(output, "You are not carrying 'brass key'.");
        StringAssert.Contains(output, "You are carrying nothing.");
    }

    private static string RunGame(params string[] commands)
    {
        var mapPath = Path.Combine(AppContext.BaseDirectory, "Adventure.json");
        var game = AdventureMapLoader.Load(mapPath);
        using var input = new StringReader(string.Join(Environment.NewLine, commands));
        using var output = new StringWriter();
        game.Run(input, output);
        return output.ToString();
    }
}
