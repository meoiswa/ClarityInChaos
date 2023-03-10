using System.Collections.Generic;
using System.Linq;
using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;

namespace ClarityInChaos
{
  // It is good to have this be disposable in general, in case you ever need it
  // to do any cleanup
  public unsafe class BattleEffectsConfigurator
  {
    private readonly ClarityInChaosPlugin plugin;

    private readonly ConfigModule* configModule;

    private readonly GroupManager* groupManager;

    public readonly List<uint> AllianceDutyIds;

    private bool firstLoop = true;
    private bool lastEnabled;

    private GroupingSize lastGroupingSize;

    public BattleEffect BattleEffectSelf
    {
      get
      {
        return (BattleEffect)configModule->GetIntValue(ConfigOption.BattleEffectSelf);
      }
      set
      {
        configModule->SetOption(ConfigOption.BattleEffectSelf, (int)value);
      }
    }

    public BattleEffect BattleEffectParty
    {
      get
      {
        return (BattleEffect)configModule->GetIntValue(ConfigOption.BattleEffectParty);
      }
      set
      {
        configModule->SetOption(ConfigOption.BattleEffectParty, (int)value);
      }
    }

    public BattleEffect BattleEffectOther
    {
      get
      {
        return (BattleEffect)configModule->GetIntValue(ConfigOption.BattleEffectOther);
      }
      set
      {
        configModule->SetOption(ConfigOption.BattleEffectOther, (int)value);
      }
    }

    public BattleEffectsConfigurator(ClarityInChaosPlugin plugin)
    {
      this.plugin = plugin;
      configModule = ConfigModule.Instance();
      groupManager = GroupManager.Instance();
      lastEnabled = plugin.Configuration.Enabled;

      AllianceDutyIds = Service.DataManager
        .GetExcelSheet<TerritoryType>(Dalamud.ClientLanguage.English)!
        .Where((r) => r.TerritoryIntendedUse is 41 or 48)
        .Select((r) => r.RowId)
        .ToList();
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
      var config = plugin.Configuration.GetconfigForGroupingSize(size);
      BattleEffectSelf = config.Self;
      BattleEffectParty = config.Party;
      BattleEffectOther = config.Other;
    }

    public void OnUpdate(Framework framework)
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
        var currentSize = GetCurrentGroupingSize();

        var configForSize = plugin.Configuration.GetconfigForGroupingSize(currentSize);

        var changed = false;

        if (currentSize != lastGroupingSize || lastEnabled != plugin.Configuration.Enabled || firstLoop)
        {
          BattleEffectSelf = configForSize.Self;
          BattleEffectParty = configForSize.Party;
          BattleEffectOther = configForSize.Other;
          changed = true;
          firstLoop = false;
        }
        else
        {
          if (BattleEffectSelf != configForSize.Self)
          {
            configForSize.Self = BattleEffectSelf;
            changed = true;
          }
          if (BattleEffectParty != configForSize.Party)
          {
            configForSize.Party = BattleEffectParty;
            changed = true;
          }
          if (BattleEffectOther != configForSize.Other)
          {
            configForSize.Other = BattleEffectOther;
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

        lastGroupingSize = currentSize;
      }

      lastEnabled = plugin.Configuration.Enabled;
    }
  }
}
