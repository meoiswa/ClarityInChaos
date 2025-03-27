using Dalamud.Configuration;
using Dalamud.Game.Config;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
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
    private IDalamudPluginInterface? pluginInterface;

    public Configuration(bool isFresh = false)
    {
      Backup = new ConfigForBackup();
      Solo = new ConfigForSolo();
      LightParty = new ConfigForLightParty();
      FullParty = new ConfigForFullParty();
      Alliance = new ConfigForAlliance();

      if (isFresh)
      {
        ApplyDefaultConfig(Backup);
        ApplyDefaultConfig(Solo);
        ApplyDefaultConfig(LightParty);
        ApplyDefaultConfig(FullParty);
        ApplyDefaultConfig(Alliance);
      }
    }

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
      this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
      pluginInterface!.SavePluginConfig(this);
    }

    private void ApplyDefaultConfig(ConfigForGroupingSize config)
    {
      Service.GameConfig.TryGet(UiConfigOption.BattleEffectSelf, out uint beSelf);
      config.Self = (BattleEffect)beSelf;
      Service.GameConfig.TryGet(UiConfigOption.BattleEffectParty, out uint beParty);
      config.Party = (BattleEffect)beParty;
      Service.GameConfig.TryGet(UiConfigOption.BattleEffectOther, out uint beOther);
      config.Other = (BattleEffect)beOther;

      Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeSelf, out uint npSelf);
      config.OwnNameplate = (NameplateVisibility)npSelf;
      Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeParty, out uint npParty);
      config.PartyNameplate = (NameplateVisibility)npParty;
      Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeAlliance, out uint npAlliance);
      config.AllianceNameplate = (NameplateVisibility)npAlliance;
      Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeOther, out uint npOthers);
      config.OthersNameplate = (NameplateVisibility)npOthers;
      Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeFriend, out uint npFriends);
      config.FriendsNameplate = (NameplateVisibility)npFriends;
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

    public NameplateVisibility OwnNameplate { get; set; }
    public NameplateVisibility PartyNameplate { get; set; }
    public NameplateVisibility AllianceNameplate { get; set; }
    public NameplateVisibility OthersNameplate { get; set; }
    public NameplateVisibility FriendsNameplate { get; set; }

    public ObjectHighlightColor OwnHighlight { get; set; }
    public ObjectHighlightColor PartyHighlight { get; set; }
    public ObjectHighlightColor OthersHighlight { get; set; }

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
