using Adventure;

var mapPath = Path.Combine(AppContext.BaseDirectory, "Adventure.json");
var game = AdventureMapLoader.Load(mapPath);

game.RegisterCommand("say", "Print a message (for example: say hello).", (context, message) =>
{
    context.Output.WriteLine(string.IsNullOrWhiteSpace(message) ? "Say what?" : $"You say: {message}");
});

game.Run();
