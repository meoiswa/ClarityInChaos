using Dalamud.IoC;
using Dalamud.Plugin.Services;

namespace ClarityInChaos
{
  public class Service
  {
#pragma warning disable CS8618
    [PluginService] public static IChatGui ChatGui { get; private set; }
    [PluginService] public static IClientState ClientState { get; private set; }
    [PluginService] public static ICondition Condition { get; private set; }
    [PluginService] public static IDataManager DataManager { get; private set; }
    [PluginService] public static IFramework Framework { get; private set; }
    [PluginService] public static IGameConfig GameConfig { get; private set; }
    [PluginService] public static IObjectTable ObjectTable { get; private set; }
    [PluginService] public static IGameInteropProvider IGameInterop { get; private set; }
    [PluginService] public static IPluginLog PluginLog { get; private set; }
#pragma warning restore CS8618
  }
}
