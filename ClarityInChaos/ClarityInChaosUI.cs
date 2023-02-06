using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace ClarityInChaos
{
  // It is good to have this be disposable in general, in case you ever need it
  // to do any cleanup
  public unsafe class ClarityInChaosUI : Window, IDisposable
  {
    private readonly ClarityInChaosPlugin plugin;

    public ClarityInChaosUI(ClarityInChaosPlugin plugin)
      : base(
        "Clarity In Chaos##ConfigWindow",
        ImGuiWindowFlags.AlwaysAutoResize
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoCollapse
      )
    {
      this.plugin = plugin;

      SizeConstraints = new WindowSizeConstraints()
      {
        MinimumSize = new Vector2(468, 0),
        MaximumSize = new Vector2(468, 1000)
      };
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
    }

    public override void OnClose()
    {
      base.OnClose();
      plugin.Configuration.IsVisible = false;
      plugin.Configuration.Save();
    }

    private void DrawSectionMasterEnable()
    {
      // can't ref a property, so use a local copy
      var enabled = plugin.Configuration.Enabled;
      if (ImGui.Checkbox("Master Enable", ref enabled))
      {
        plugin.Configuration.Enabled = enabled;
        plugin.Configuration.Save();
      }

      var green = new Vector4(0, 255, 0, 255);

      ImGui.TextColored(green, $"Current BattleEffects: {plugin.BattleEffectsConfigurator.GetCurrentGroupingSize()}");

      if (plugin.BattleEffectsConfigurator.IsTerritoryAllianceLike())
      {
        ImGui.SameLine();
        ImGui.TextColored(green, $"(Misc. Duty detected)");
      }

      ImGui.Indent();
      ImGui.TextColored(green, $"Self: {plugin.BattleEffectsConfigurator.BattleEffectSelf}");
      ImGui.TextColored(green, $"Party: {plugin.BattleEffectsConfigurator.BattleEffectParty}");
      ImGui.TextColored(green, $"Other: {plugin.BattleEffectsConfigurator.BattleEffectOther}");
      ImGui.Unindent();
    }

    private bool DrawCheckboxesGroup(ref ConfigForGroupingSize config)
    {
      var changed = false;
      var self = config.Self;
      var party = config.Party;
      var other = config.Other;

      ImGui.PushID($"{config.Size}");

      ImGui.BeginTable("Table", 4);


      if (DrawCheckboxesLine($"Own", ref self))
      {
        config.Self = self;
        changed = true;
      }
      if (DrawCheckboxesLine($"Party", ref party))
      {
        config.Party = party;
        changed = true;
      }
      if (DrawCheckboxesLine($"Others (excl. PvP)", ref other))
      {
        config.Other = other;
        changed = true;
      }

      ImGui.EndTable();

      ImGui.PopID();

      return changed;
    }

    private bool DrawCheckboxesLine(string label, ref BattleEffect effect)
    {
      var changed = false;

      ImGui.TableNextRow();
      ImGui.TableSetColumnIndex(0);
      ImGui.Text(label);

      ImGui.PushID(label);

      ImGui.TableSetColumnIndex(1);
      if (ImGui.RadioButton($"All", effect is BattleEffect.All))
      {
        effect = BattleEffect.All;
        changed = true;
      }

      ImGui.TableSetColumnIndex(2);
      if (ImGui.RadioButton($"Limited", effect is BattleEffect.Limited))
      {
        effect = BattleEffect.Limited;
        changed = true;
      }

      ImGui.TableSetColumnIndex(3);
      if (ImGui.RadioButton($"None", effect is BattleEffect.None))
      {
        effect = BattleEffect.None;
        changed = true;
      }

      ImGui.PopID();

      return changed;
    }

    private void DrawGroupingSizeGroup(GroupingSize size)
    {
      var configForGroupSize = plugin.Configuration.GetconfigForGroupingSize(size);
      if (DrawCheckboxesGroup(ref configForGroupSize))
      {
        plugin.Configuration.Save();
      }
    }

    private void DrawMatrixSection()
    {
      if (ImGui.CollapsingHeader("Solo"))
      {
        ImGui.Indent();

        DrawGroupingSizeGroup(GroupingSize.Solo);

        ImGui.Unindent();
      }

      if (ImGui.CollapsingHeader("Light Party (4-man)"))
      {
        ImGui.Indent();

        DrawGroupingSizeGroup(GroupingSize.LightParty);

        ImGui.Unindent();
      }

      if (ImGui.CollapsingHeader("Full Party (8-man)"))
      {
        ImGui.Indent();

        DrawGroupingSizeGroup(GroupingSize.FullParty);

        ImGui.Unindent();
      }

      if (ImGui.CollapsingHeader("Alliance (24-man)"))
      {
        ImGui.Indent();

        DrawGroupingSizeGroup(GroupingSize.Alliance);

        ImGui.Unindent();
      }
    }

    public void DrawDebugSection()
    {
      if (ImGui.CollapsingHeader("Debug Options"))
      {
        ImGui.Indent();

        ImGui.TextWrapped("Use these to test your settings.");

        var debugMessages = plugin.Configuration.DebugMessages;
        if (ImGui.Checkbox("Debug Messages", ref debugMessages))
        {
          plugin.Configuration.DebugMessages = debugMessages;
          plugin.Configuration.Save();
        }

        var psize = plugin.Configuration.DebugPartySize;
        var forcePSize = plugin.Configuration.DebugForcePartySize;
        if (ImGui.Checkbox("Force Party Size", ref forcePSize))
        {
          plugin.Configuration.DebugForcePartySize = forcePSize;
          plugin.Configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.InputInt("##psize", ref psize))
        {
          plugin.Configuration.DebugPartySize = Math.Max(psize, 0);
          plugin.Configuration.Save();
        }

        ImGui.Text($"Backup BattleEffects:");
        ImGui.Indent();
        ImGui.Text($"Self: {plugin.Configuration.Backup.Self}");
        ImGui.Text($"Party: {plugin.Configuration.Backup.Party}");
        ImGui.Text($"Other: {plugin.Configuration.Backup.Other}");
        ImGui.Unindent();

        ImGui.Unindent();
      }
    }

    public override void Draw()
    {
      DrawSectionMasterEnable();

      ImGui.Separator();

      DrawMatrixSection();

      ImGui.Separator();

      DrawDebugSection();
    }
  }
}
