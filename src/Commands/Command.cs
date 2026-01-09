using System;
using CommandSystem;

namespace Template.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class ExampleCommand : ICommand, IUsageProvider
{
    public string Command => "example";
    public string[] Aliases => ["ex"];
    public string Description => "An example command for the plugin.";
    public string[] Usage => ["<none>"];

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var cfg = Plugin.Instance?.Config;
        if (cfg is null)
        {
            response = "Config introuvable (plugin non initialisé).";
            return false;
        }

        response = "Command executed successfully!";
        return true;
    }
}