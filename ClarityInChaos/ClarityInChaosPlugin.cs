using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace ClarityInChaos
{
  public sealed class ClarityInChaosPlugin : IDalamudPlugin
  {
    public string Name => "ClarityInChaos";

    private const string commandName = "/cic";

    public IDalamudPluginInterface PluginInterface { get; init; }
    public ICommandManager CommandManager { get; init; }
    public Configuration Configuration { get; init; }
    public WindowSystem WindowSystem { get; init; }
    public UiSettingsConfigurator BattleEffectsConfigurator { get; init; }
    public ClarityInChaosUI Window { get; init; }

    public bool BoundByDuty => Configuration.DebugForceInDuty ||
                               (Service.Condition[ConditionFlag.BoundByDuty]
                                && !Service.Condition[ConditionFlag.BetweenAreas]
                                && !Service.Condition[ConditionFlag.OccupiedInCutSceneEvent]);

    public ClarityInChaosPlugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager)
    {
      pluginInterface.Create<Service>();
      
      PluginInterface = pluginInterface;
      CommandManager = commandManager;
      WindowSystem = new("ClarityInChaosPlugin");

      Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration(true);
      Configuration.Initialize(PluginInterface);

      BattleEffectsConfigurator = new UiSettingsConfigurator(this);

      Window = new ClarityInChaosUI(this)
      {
        IsOpen = Configuration.IsVisible
      };

      WindowSystem.AddWindow(Window);

      CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
      {
        HelpMessage = "opens the configuration window"
      });

      Service.Framework.Update += BattleEffectsConfigurator.OnUpdate;
      PluginInterface.UiBuilder.Draw += DrawUI;
      PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }

    public void Dispose()
    {

      PluginInterface.UiBuilder.Draw -= DrawUI;
      PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;

      Service.Framework.Update -= BattleEffectsConfigurator.OnUpdate;

      BattleEffectsConfigurator.Restore();

      WindowSystem.RemoveAllWindows();

      CommandManager.RemoveHandler(commandName);
    }

    public void PrintDebug(string message)
    {
      Service.PluginLog.Debug(message);
      if (Configuration.DebugMessages)
      {
        Service.ChatGui.Print($"Clarity In Chaos: {message}");
      }
    }

    private void SetVisible(bool isVisible)
    {
      Configuration.IsVisible = isVisible;
      Configuration.Save();

      Window.IsOpen = Configuration.IsVisible;
    }

    private void OnCommand(string command, string args)
    {
      SetVisible(!Configuration.IsVisible);
    }

    private void DrawUI()
    {
      WindowSystem.Draw();
    }

    private void DrawConfigUI()
    {
      SetVisible(!Configuration.IsVisible);
    }
  }
}
