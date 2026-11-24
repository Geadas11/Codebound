using Codebound.Services;

// `Codebound.Services backup <file>`: an online copy of the database (safe while the service runs).
if (args.Length >= 2 && args[0] == "backup")
{
    var cfg = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true).AddEnvironmentVariables().Build();
    var o = new ServiceOptions(); cfg.GetSection("Codebound").Bind(o);
    new Db(o).Backup(args[1]);
    Console.WriteLine($"Backed up {o.Database} to {args[1]}");
    return;
}

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment()) builder.Logging.AddJsonConsole();   // production: one JSON object per line for log collectors
App.Build(builder).Run();

public partial class Program { }
