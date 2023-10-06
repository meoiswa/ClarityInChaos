using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Config;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using Lumina.Excel.GeneratedSheets;

namespace ClarityInChaos
{
  // It is good to have this be disposable in general, in case you ever need it
  // to do any cleanup
  public unsafe class BattleEffectsConfigurator
  {
    private readonly ClarityInChaosPlugin plugin;

    private readonly GroupManager* groupManager;

    public readonly List<uint> AllianceDutyIds;

    private ConfigForGroupingSize lastActiveConfig;

    private bool lastEnabled;

    private bool lastDebugDuty;

    public BattleEffect BattleEffectSelf
    {
      get
      {
        Service.GameConfig.TryGet(UiConfigOption.BattleEffectSelf, out uint beSelf);
        return (BattleEffect)beSelf;
      }
      set
      {
        Service.GameConfig.Set(UiConfigOption.BattleEffectSelf, (uint)value);
      }
    }

    public BattleEffect BattleEffectParty
    {
      get
      {
        Service.GameConfig.TryGet(UiConfigOption.BattleEffectParty, out uint beParty);
        return (BattleEffect)beParty;
      }
      set
      {
        Service.GameConfig.Set(UiConfigOption.BattleEffectParty, (uint)value);
      }
    }

    public BattleEffect BattleEffectOther
    {
      get
      {
        Service.GameConfig.TryGet(UiConfigOption.BattleEffectOther, out uint beOther);
        return (BattleEffect)beOther;
      }
      set
      {
        Service.GameConfig.Set(UiConfigOption.BattleEffectOther, (uint)value);
      }
    }

    public BattleEffectsConfigurator(ClarityInChaosPlugin plugin)
    {
      this.plugin = plugin;
      groupManager = GroupManager.Instance();
      lastEnabled = plugin.Configuration.Enabled;
      lastDebugDuty = plugin.Configuration.DebugForceInDuty;

      AllianceDutyIds = Service.DataManager
        .GetExcelSheet<TerritoryType>(Dalamud.ClientLanguage.English)!
        .Where((r) => r.TerritoryIntendedUse is 41 or 48)
        .Select((r) => r.RowId)
        .ToList();

      lastActiveConfig = plugin.Configuration.GetConfigForGroupingSize(GetCurrentGroupingSize(), plugin.BoundByDuty);
    }

    public bool IsTerritoryAllianceLike()
    {
      return AllianceDutyIds.FindIndex((r) => r == plugin.ClientState.TerritoryType) >= 0;
    }

    public GroupingSize GetCurrentGroupingSize()
    {
      var memberCount = groupManager->MemberCount;
      var allianceFlags = groupManager->AllianceFlags;

      if (plugin.Configuration.DebugForcePartySize)
      {
        memberCount = (byte)plugin.Configuration.DebugPartySize;
        if (memberCount > 8)
        {
          allianceFlags = 1;
        }
      }

      var currentSize = memberCount switch
      {
        <= 1 => GroupingSize.Solo,
        > 0 and <= 4 => GroupingSize.LightParty,
        _ when allianceFlags is not 0 => GroupingSize.Alliance,
        _ => GroupingSize.FullParty
      };

      if (IsTerritoryAllianceLike())
      {
        currentSize = GroupingSize.Alliance;
      }

      return currentSize;
    }

    public void Restore()
    {
      var backup = plugin.Configuration.Backup;
      BattleEffectSelf = backup.Self;
      BattleEffectParty = backup.Party;
      BattleEffectOther = backup.Other;
    }

    public void UIChange(GroupingSize size)
    {

      var config = plugin.Configuration.GetConfigForGroupingSize(size, plugin.BoundByDuty);
      BattleEffectSelf = config.Self;
      BattleEffectParty = config.Party;
      BattleEffectOther = config.Other;
    }

    public void OnUpdate(IFramework framework)
    {
      if (!plugin.Configuration.Enabled && lastEnabled)
      {
        if (BattleEffectSelf != plugin.Configuration.Backup.Self)
        {
          BattleEffectSelf = plugin.Configuration.Backup.Self;
        }
        if (BattleEffectParty != plugin.Configuration.Backup.Party)
        {
          BattleEffectParty = plugin.Configuration.Backup.Party;
        }
        if (BattleEffectOther != plugin.Configuration.Backup.Other)
        {
          BattleEffectOther = plugin.Configuration.Backup.Other;
        }
      }
      else if (!plugin.Configuration.Enabled && !lastEnabled)
      {
        if (plugin.Configuration.Backup.Self != BattleEffectSelf)
        {
          plugin.Configuration.Backup.Self = BattleEffectSelf;
        }
        if (plugin.Configuration.Backup.Party != BattleEffectParty)
        {
          plugin.Configuration.Backup.Party = BattleEffectParty;
        }
        if (plugin.Configuration.Backup.Other != BattleEffectOther)
        {
          plugin.Configuration.Backup.Other = BattleEffectOther;
        }
      }
      else
      {
        var activeConfig = plugin.Configuration.GetConfigForGroupingSize(GetCurrentGroupingSize(), plugin.BoundByDuty);

        var changed = false;

        if (activeConfig != lastActiveConfig)
        {
          BattleEffectSelf = activeConfig.Self;
          BattleEffectParty = activeConfig.Party;
          BattleEffectOther = activeConfig.Other;
          changed = true;
        }
        else
        {
          if (BattleEffectSelf != activeConfig.Self)
          {
            activeConfig.Self = BattleEffectSelf;
            changed = true;
          }
          if (BattleEffectParty != activeConfig.Party)
          {
            activeConfig.Party = BattleEffectParty;
            changed = true;
          }
          if (BattleEffectOther != activeConfig.Other)
          {
            activeConfig.Other = BattleEffectOther;
            changed = true;
          }

          if (changed)
          {
            plugin.Configuration.Save();
          }
        }

        if (changed)
        {
          plugin.PrintDebug("Updated BattleEffects!");
        }

        lastActiveConfig = activeConfig;
      }
    }
  }
}
