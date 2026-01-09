using System.CommandLine;

namespace SCPSLTemplateGenerator;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Commande racine
        var rootCommand = new RootCommand("CLI tool for generating SCP:SL LabAPI plugin templates");

        // Commande 'new' pour créer un nouveau plugin
        var newCommand = new Command("new", "Create a new SCP:SL plugin from template")
        {
            // Arguments requis
            new Argument<string>("name", "The name of the plugin to create")
        };

        // Options pour personnaliser le template
        var authorOption = new Option<string>(
            aliases: ["--author", "-a"],
            description: "The author of the plugin",
            getDefaultValue: () => Environment.UserName);

        var descriptionOption = new Option<string>(
            aliases: ["--description", "-d"],
            description: "The description of the plugin",
            getDefaultValue: () => "A SCP:SL LabAPI plugin");

        var versionOption = new Option<string>(
            aliases: ["--version", "-v"],
            description: "The initial version of the plugin",
            getDefaultValue: () => "1, 0, 0");

        var outputOption = new Option<DirectoryInfo?>(
            aliases: ["--output", "-o"],
            description: "Output directory (defaults to current directory)",
            getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory()));

        var forceOption = new Option<bool>(
            aliases: ["--force", "-f"],
            description: "Overwrite existing files if they exist",
            getDefaultValue: () => false);

        // Ajouter les options à la commande
        newCommand.AddOption(authorOption);
        newCommand.AddOption(descriptionOption);
        newCommand.AddOption(versionOption);
        newCommand.AddOption(outputOption);
        newCommand.AddOption(forceOption);

        // Handler pour la commande 'new'
        newCommand.SetHandler(async (name, author, description, version, output, force) =>
        {
            try
            {
                var generator = new TemplateGenerator();
                var result = await generator.GeneratePluginAsync(new PluginGenerationOptions
                {
                    Name = name,
                    Author = author,
                    Description = description,
                    Version = version,
                    OutputDirectory = output,
                    Force = force
                });

                if (result.IsSuccess)
                {
                    ConsoleHelper.WriteSuccess($"[SUCCESS] Plugin '{name}' created successfully!");
                    ConsoleHelper.WriteInfo($"[INFO] Location: {result.OutputPath}");
                    Console.WriteLine();
                    Console.WriteLine("Next steps:");
                    Console.WriteLine($"  cd {result.OutputPath}");
                    Console.WriteLine($"  dotnet build");
                    Console.WriteLine();
                }
                else
                {
                    ConsoleHelper.WriteError($"[ERROR] Error: {result.ErrorMessage}");
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"[ERROR] Unexpected error: {ex.Message}");
                Environment.Exit(1);
            }
        }, 
        newCommand.Arguments[0] as Argument<string> ?? throw new InvalidOperationException("Plugin name argument is required"), 
        authorOption, descriptionOption, versionOption, outputOption, forceOption);

        // Ajouter la commande au root command
        rootCommand.AddCommand(newCommand);

        // Version command
        var versionCommand = new Command("version", "Show version information");
        versionCommand.SetHandler(() =>
        {
            var version = typeof(Program).Assembly.GetName().Version;
            Console.WriteLine($"SCPSL Template Generator v{version}");
        });
        rootCommand.AddCommand(versionCommand);

        return await rootCommand.InvokeAsync(args);
    }
}