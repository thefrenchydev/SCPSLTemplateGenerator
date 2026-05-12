namespace Template;

using System;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins.Enums;
using Template.Events;

public class Plugin : LabApi.Loader.Features.Plugins.Plugin<Config>
{
    public static Plugin Instance { get; private set; } = null!;
    public override string Name => "Template";
    public override string Author => "TheFrenchyDev";
    public override string Description => "This is a template plugin";
    public override Version Version => new(1, 0, 0);
    public override Version RequiredApiVersion => new(LabApiProperties.CompiledVersion);
    public override LoadPriority Priority => LoadPriority.High;
    private EventsHandler Events { get; } = new();

    public override void Enable()
    {
        if (Config is null)
            throw new Exception("Config is null!");
        
        if (Config.Debug)
            Logger.Debug("Debug mode enabled.");
            
        Instance = this;
        CustomHandlersManager.RegisterEventsHandler(Events);
    }

    public override void Disable()
    {
        Instance = null!;
        CustomHandlersManager.UnregisterEventsHandler(Events);
    }
}
