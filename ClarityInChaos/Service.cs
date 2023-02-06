using Dalamud.Game;
using Dalamud.Game.Gui;
using Dalamud.IoC;

namespace ClarityInChaos
{
  public class Service
  {
#pragma warning disable CS8618
    [PluginService] public static Framework Framework { get; private set; }
    [PluginService] public static ChatGui ChatGui { get; private set; }
#pragma warning restore CS8618
  }
}