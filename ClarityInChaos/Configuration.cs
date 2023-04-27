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

    public ConfigForBackup Backup { get; init; }
    public ConfigForSolo Solo { get; init; }
    public ConfigForLightParty LightParty { get; init; }
    public ConfigForFullParty FullParty { get; init; }
    public ConfigForAlliance Alliance { get; init; }

    public bool DebugMessages = false;

    public bool DebugForcePartySize = false;
    public int DebugPartySize = 0;
    public bool DebugForceInDuty = false;

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private DalamudPluginInterface? pluginInterface;

    public Configuration()
    {
      Backup = new ConfigForBackup();
      ApplyDefaultConfig(Backup);
      Solo = new ConfigForSolo();
      ApplyDefaultConfig(Solo);
      LightParty = new ConfigForLightParty();
      ApplyDefaultConfig(LightParty);
      FullParty = new ConfigForFullParty();
      ApplyDefaultConfig(FullParty);
      Alliance = new ConfigForAlliance();
      ApplyDefaultConfig(Alliance);
    }

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
      this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
      pluginInterface!.SavePluginConfig(this);
    }

    private void ApplyDefaultConfig(ConfigForGroupingSize config)
    {
      config.Self = (BattleEffect)ConfigModule.Instance()->GetIntValue(ConfigOption.BattleEffectSelf);
      config.Party = (BattleEffect)ConfigModule.Instance()->GetIntValue(ConfigOption.BattleEffectParty);
      config.Other = (BattleEffect)ConfigModule.Instance()->GetIntValue(ConfigOption.BattleEffectOther);
    }

    private bool InDutyFilter(ConfigForGroupingSize config, bool inDuty)
    {
      return !config.OnlyInDuty || (config.OnlyInDuty && inDuty);
    }

    private ConfigForGroupingSize GetConfigForGroupingSize(GroupingSize size)
    {
      return size switch
      {
        GroupingSize.Solo => Solo,
        GroupingSize.LightParty => LightParty,
        GroupingSize.FullParty => FullParty,
        GroupingSize.Alliance => Alliance,
        _ => Backup,
      };
    }

    private ConfigForGroupingSize GetConfigForGroupingSizeNotInDuty(GroupingSize size)
    {
      var config = GetConfigForGroupingSize(size);
      if (config.OnlyInDuty)
      {
        if (size == GroupingSize.Backup)
        {
          return Backup;
        }
        else
        {
          return GetConfigForGroupingSizeNotInDuty(size - 1);
        }
      }
      return config;
    }

    public ConfigForGroupingSize GetConfigForGroupingSize(GroupingSize size, bool inDuty)
    {
      if (inDuty)
      {
        return GetConfigForGroupingSize(size);
      }
      else
      {
        return GetConfigForGroupingSizeNotInDuty(size);
      }
    }
  }

  public abstract class ConfigForGroupingSize
  {
    public abstract GroupingSize Size { get; }
    public BattleEffect Self { get; set; }
    public BattleEffect Party { get; set; }
    public BattleEffect Other { get; set; }

    public bool OnlyInDuty { get; set; }
  }

  public class ConfigForBackup : ConfigForGroupingSize
  {
    public override GroupingSize Size => GroupingSize.Backup;
  }

  public class ConfigForSolo : ConfigForGroupingSize
  {
    public override GroupingSize Size => GroupingSize.Solo;
  }

  public class ConfigForLightParty : ConfigForGroupingSize
  {
    public override GroupingSize Size => GroupingSize.LightParty;
  }

  public class ConfigForFullParty : ConfigForGroupingSize
  {
    public override GroupingSize Size => GroupingSize.FullParty;
  }

  public class ConfigForAlliance : ConfigForGroupingSize
  {
    public override GroupingSize Size => GroupingSize.Alliance;
  }
}
