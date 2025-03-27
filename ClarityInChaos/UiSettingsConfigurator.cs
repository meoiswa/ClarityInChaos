using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Config;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Lumina.Excel.Sheets;

namespace ClarityInChaos
{
  public unsafe class UiSettingsConfigurator
  {
    private readonly ClarityInChaosPlugin plugin;

    private readonly GroupManager* groupManager;

    public readonly List<uint> AllianceDutyIds;

    private ConfigForGroupingSize? lastActiveConfig = null;

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

    public NameplateVisibility OwnNameplate
    {
      get
      {
        Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeSelf, out uint npSelf);
        return (NameplateVisibility)npSelf;
      }
      set
      {
        Service.GameConfig.Set(UiConfigOption.NamePlateDispTypeSelf, (uint)value);
      }
    }

    public NameplateVisibility PartyNameplate
    {
      get
      {
        Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeParty, out uint npParty);
        return (NameplateVisibility)npParty;
      }
      set
      {
        Service.GameConfig.Set(UiConfigOption.NamePlateDispTypeParty, (uint)value);
      }
    }

    public NameplateVisibility AllianceNameplate
    {
      get
      {
        Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeAlliance, out uint npAlliance);
        return (NameplateVisibility)npAlliance;
      }
      set
      {
        Service.GameConfig.Set(UiConfigOption.NamePlateDispTypeAlliance, (uint)value);
      }
    }

    public NameplateVisibility OthersNameplate
    {
      get
      {
        Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeOther, out uint npOthers);
        return (NameplateVisibility)npOthers;
      }
      set
      {
        Service.GameConfig.Set(UiConfigOption.NamePlateDispTypeOther, (uint)value);
      }
    }

    public NameplateVisibility FriendsNameplate
    {
      get
      {
        Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeFriend, out uint npFriends);
        return (NameplateVisibility)npFriends;
      }
      set
      {
        Service.GameConfig.Set(UiConfigOption.NamePlateDispTypeFriend, (uint)value);
      }
    }

    public UiSettingsConfigurator(ClarityInChaosPlugin plugin)
    {
      this.plugin = plugin;
      groupManager = GroupManager.Instance();

      AllianceDutyIds = Service.DataManager
        .GetExcelSheet<TerritoryType>(Dalamud.Game.ClientLanguage.English)!
        .Where((r) => r.TerritoryIntendedUse.RowId is 41 or 48)
        .Select((r) => r.RowId)
        .ToList();
    }

    public bool IsTerritoryAllianceLike()
    {
      return AllianceDutyIds.FindIndex((r) => r == Service.ClientState.TerritoryType) >= 0;
    }

    public GroupingSize GetCurrentGroupingSize()
    {
      var memberCount = groupManager->MainGroup.MemberCount;
      var allianceFlags = groupManager->MainGroup.AllianceFlags;

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
      OwnNameplate = backup.OwnNameplate;
      PartyNameplate = backup.PartyNameplate;
      AllianceNameplate = backup.AllianceNameplate;
      OthersNameplate = backup.OthersNameplate;
      FriendsNameplate = backup.FriendsNameplate;
    }

    public void UIChange(GroupingSize size)
    {

      var config = plugin.Configuration.GetConfigForGroupingSize(size, plugin.BoundByDuty);
      BattleEffectSelf = config.Self;
      BattleEffectParty = config.Party;
      BattleEffectOther = config.Other;
      OwnNameplate = config.OwnNameplate;
      PartyNameplate = config.PartyNameplate;
      AllianceNameplate = config.AllianceNameplate;
      OthersNameplate = config.OthersNameplate;
      FriendsNameplate = config.FriendsNameplate;
      ClearHighlights();
    }

    public void OnUpdate(IFramework framework)
    {
      var activeConfig = plugin.Configuration.GetConfigForGroupingSize(GetCurrentGroupingSize(), plugin.BoundByDuty);

      if (!plugin.Configuration.Enabled)
      {
        activeConfig = plugin.Configuration.Backup;
      }

      var changed = false;

      if (activeConfig != lastActiveConfig)
      {
        BattleEffectSelf = activeConfig.Self;
        BattleEffectParty = activeConfig.Party;
        BattleEffectOther = activeConfig.Other;
        OwnNameplate = activeConfig.OwnNameplate;
        PartyNameplate = activeConfig.PartyNameplate;
        AllianceNameplate = activeConfig.AllianceNameplate;
        OthersNameplate = activeConfig.OthersNameplate;
        FriendsNameplate = activeConfig.FriendsNameplate;
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
        if (OwnNameplate != activeConfig.OwnNameplate)
        {
          activeConfig.OwnNameplate = OwnNameplate;
          changed = true;
        }
        if (PartyNameplate != activeConfig.PartyNameplate)
        {
          activeConfig.PartyNameplate = PartyNameplate;
          changed = true;
        }
        if (AllianceNameplate != activeConfig.AllianceNameplate)
        {
          activeConfig.AllianceNameplate = AllianceNameplate;
          changed = true;
        }
        if (OthersNameplate != activeConfig.OthersNameplate)
        {
          activeConfig.OthersNameplate = OthersNameplate;
          changed = true;
        }
        if (FriendsNameplate != activeConfig.FriendsNameplate)
        {
          activeConfig.FriendsNameplate = FriendsNameplate;
          changed = true;
        }

        if (changed)
        {
          plugin.Configuration.Save();
        }
      }

      if (changed)
      {
        ClearHighlights();
        plugin.PrintDebug("Updated UiSettings!");
      }

      if (plugin.Configuration.Enabled)
      {
        UpdateHighlights(activeConfig);
      }

      lastActiveConfig = activeConfig;
    }

    public void ClearHighlights()
    {
      var pcKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player;
      foreach (var gameObject in Service.ObjectTable.Where(o => o.ObjectKind == pcKind))
      {
        ApplyHighlight(gameObject?.Address, ObjectHighlightColor.None);
      }
    }

    public void UpdateHighlights(ConfigForGroupingSize config)
    {
      var pcKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player;
      var pcs = Service.ObjectTable.Where(o => o.ObjectKind == pcKind && o.EntityId != Service.ClientState.LocalPlayer?.EntityId);
      var party = groupManager->MainGroup.PartyMembers.ToArray();
      var partyMembers = pcs.Where(o => party.Any(p => p.EntityId == o.EntityId));
      var others = pcs.Where(o => !party.Any(p => p.EntityId == o.EntityId));

      if (config.OthersHighlight != ObjectHighlightColor.None)
      {
        foreach (var gameObject in others)
        {
          ApplyHighlight(gameObject?.Address, config.OthersHighlight);
        }
      }

      if (config.PartyHighlight != ObjectHighlightColor.None)
      {
        foreach (var gameObject in partyMembers)
        {
          ApplyHighlight(gameObject?.Address, config.PartyHighlight);
        }
      }

      if (config.OwnHighlight != ObjectHighlightColor.None)
      {
        ApplyHighlight(Service.ClientState.LocalPlayer?.Address, config.OwnHighlight);
      }
    }

    public void ApplyHighlight(IntPtr? gameObject, ObjectHighlightColor color)
    {
      if (gameObject.HasValue && gameObject != IntPtr.Zero)
      {
        var ptr = (GameObject*)gameObject;
        ptr->Highlight(color);
      }
    }
  }
}
