using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace ClarityInChaos
{
  // It is good to have this be disposable in general, in case you ever need it
  // to do any cleanup
  public unsafe class BattleEffectsConfigurator
  {
    private readonly ClarityInChaosPlugin plugin;

    private readonly ConfigModule* configModule;

    private readonly GroupManager* groupManager;

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

      return currentSize;
    }

    public void Restore()
    {
      var backup = plugin.Configuration.Backup;
      BattleEffectSelf = backup.Self;
      BattleEffectParty = backup.Party;
      BattleEffectOther = backup.Other;
    }

    public void OnUpdate(Framework framework)
    {
      if (!plugin.Configuration.Enabled)
      {
        return;
      }

      var currentSize = GetCurrentGroupingSize();

      var configForSize = plugin.Configuration.GetconfigForGroupingSize(currentSize);

      var changed = false;

      if (BattleEffectSelf != configForSize.Self)
      {
        BattleEffectSelf = configForSize.Self;
        changed = true;
      }
      if (BattleEffectParty != configForSize.Party)
      {
        BattleEffectParty = configForSize.Party;
        changed = true;
      }
      if (BattleEffectOther != configForSize.Other)
      {
        BattleEffectOther = configForSize.Other;
        changed = true;
      }

      if (changed)
      {
        plugin.PrintDebug("Updated BattleEffects!");
      }
    }
  }
}
