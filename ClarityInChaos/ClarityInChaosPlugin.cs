using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace ClarityInChaos
{
  public sealed class ClarityInChaosPlugin : IDalamudPlugin
  {
    public string Name => "ClarityInChaos";

    private const string commandName = "/cic";

    public DalamudPluginInterface PluginInterface { get; init; }
    public CommandManager CommandManager { get; init; }
    public ChatGui ChatGui { get; init; }
    public ClientState ClientState { get; init; }
    public Configuration Configuration { get; init; }
    public WindowSystem WindowSystem { get; init; }
    public BattleEffectsConfigurator BattleEffectsConfigurator { get; init; }
    public Condition Condition { get; init; }
    public ClarityInChaosUI Window { get; init; }

    public bool BoundByDuty => Configuration.DebugForceInDuty ||
                               (Condition[ConditionFlag.BoundByDuty]
                                && !Condition[ConditionFlag.BetweenAreas]
                                && !Condition[ConditionFlag.OccupiedInCutSceneEvent]);

    public ClarityInChaosPlugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager,
        [RequiredVersion("1.0")] ChatGui chatGui,
        [RequiredVersion("1.0")] ClientState clientState)
    {
      pluginInterface.Create<Service>();
      Condition = Service.Condition;

      PluginInterface = pluginInterface;
      CommandManager = commandManager;
      ChatGui = chatGui;
      ClientState = clientState;
      WindowSystem = new("ClarityInChaosPlugin");

      Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
      Configuration.Initialize(PluginInterface);

      BattleEffectsConfigurator = new BattleEffectsConfigurator(this);

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
      PluginLog.LogDebug(message);
      if (Configuration.DebugMessages)
      {
        ChatGui.Print($"Clarity In Chaos: {message}");
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
