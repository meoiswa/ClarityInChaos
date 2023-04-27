using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Gui;
using Dalamud.IoC;

namespace ClarityInChaos
{
  public class Service
  {
#pragma warning disable CS8618
    [PluginService] public static ChatGui ChatGui { get; private set; }
    [PluginService] public static ClientState ClientState { get; private set; } 
    [PluginService] public static Condition Condition { get; private set; }
    [PluginService] public static DataManager DataManager { get; private set; }
    [PluginService] public static Framework Framework { get; private set; }
#pragma warning restore CS8618
  }
}
