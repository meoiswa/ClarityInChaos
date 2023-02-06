using Dalamud.Configuration;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;

namespace ClarityInChaos
{
  [Serializable]
  public unsafe class Configuration : IPluginConfiguration
  {
    public int Version { get; set; } = 0;

    public bool IsVisible { get; set; } = true;

    public bool Enabled { get; set; } = true;

    public ConfigForGroupingSize Backup { get; set; } = DefaultConfigForGroupingSize(GroupingSize.Solo);
    public ConfigForGroupingSize Solo { get; set; } = DefaultConfigForGroupingSize(GroupingSize.Solo);
    public ConfigForGroupingSize LightParty { get; set; } = DefaultConfigForGroupingSize(GroupingSize.LightParty);
    public ConfigForGroupingSize FullParty { get; set; } = DefaultConfigForGroupingSize(GroupingSize.FullParty);
    public ConfigForGroupingSize Alliance { get; set; } = DefaultConfigForGroupingSize(GroupingSize.Alliance);
    
    public bool DebugMessages = false;

    public bool DebugForcePartySize = false;
    public int DebugPartySize = 0;

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private DalamudPluginInterface? pluginInterface;
    public void Initialize(DalamudPluginInterface pluginInterface) => this.pluginInterface = pluginInterface;
    public void Save()
    {
      pluginInterface!.SavePluginConfig(this);
    }

    public static ConfigForGroupingSize DefaultConfigForGroupingSize(GroupingSize size)
    {
      return new ConfigForGroupingSize(
        size,
        (BattleEffect) ConfigModule.Instance()->GetIntValue(ConfigOption.BattleEffectSelf),
        (BattleEffect) ConfigModule.Instance()->GetIntValue(ConfigOption.BattleEffectParty),
        (BattleEffect) ConfigModule.Instance()->GetIntValue(ConfigOption.BattleEffectOther)
      );
    }

    public ConfigForGroupingSize GetconfigForGroupingSize(GroupingSize size)
    {
      return size switch
      {
        GroupingSize.LightParty => LightParty,
        GroupingSize.FullParty => FullParty,
        GroupingSize.Alliance => Alliance,
        _ => Solo,
      };
    }
  }

  public class ConfigForGroupingSize
  {
    public GroupingSize Size { get; init; }
    public BattleEffect Self { get; set; }
    public BattleEffect Party {get; set; }
    public BattleEffect Other { get; set; }

    public ConfigForGroupingSize(GroupingSize size, BattleEffect self, BattleEffect party, BattleEffect other)
    {
      Size = size;
      Self = self;
      Party = party;
      Other = other;
    }
  }
}
