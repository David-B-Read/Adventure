using Adventure;

var townSquare = new GameRoom("Town Square", "A quiet square sits beneath the shadow of an old clock tower.")
    .AddExit("north", "Forest Path");
var forestPath = new GameRoom("Forest Path", "Tall trees crowd the path. A trail leads back toward town.")
    .AddExit("south", "Town Square");

var game = new AdventureGame(
    "The Clocktower Adventure",
    "Explore the area. Type 'help' for commands, or 'quit' to leave.",
    "Town Square",
    [townSquare, forestPath]);

game.RegisterCommand("say", "Print a message (for example: say hello).", (context, message) =>
{
    context.Output.WriteLine(string.IsNullOrWhiteSpace(message) ? "Say what?" : $"You say: {message}");
});

game.Run();
