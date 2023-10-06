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

    private void PrettyEffect(string label, BattleEffect effect)
    {
      ImGui.Text($"{label}:");
      ImGui.SameLine();
      ImGui.TextColored(ColorForEffect(effect), effect.ToString());
    }

    private Vector4 ColorForEffect(BattleEffect effect)
    {
      Vector4 color = new Vector4(255, 255, 255, 255);
      switch (effect)
      {
        case BattleEffect.All:
          color = new Vector4(0, 255, 0, 255);
          break;
        case BattleEffect.Limited:
          color = new Vector4(255, 255, 0, 255);
          break;
        case BattleEffect.None:
          color = new Vector4(255, 0, 0, 255);
          break;
      }
      return color;
    }

    private void DrawSectionMasterEnable(ConfigForGroupingSize activeConfig)
    {
      // can't ref a property, so use a local copy
      var enabled = plugin.Configuration.Enabled;
      if (ImGui.Checkbox("Master Enable", ref enabled))
      {
        plugin.Configuration.Enabled = enabled;
        plugin.Configuration.Save();
      }

      ImGui.Separator();

      var green = new Vector4(0, 255, 0, 255);

      if (plugin.Configuration.Enabled)
      {
        ImGui.TextColored(green, $"Current BattleEffects: {activeConfig.Size}");
      }
      else
      {
        ImGui.Text($"Current BattleEffects: Saved In-Game Settings");
      }

      if (plugin.BattleEffectsConfigurator.IsTerritoryAllianceLike())
      {
        ImGui.SameLine();
        ImGui.TextColored(green, $"(Misc. Duty detected)");
      }

      ImGui.Indent();
      PrettyEffect("Self", plugin.BattleEffectsConfigurator.BattleEffectSelf);
      PrettyEffect("Party", plugin.BattleEffectsConfigurator.BattleEffectParty);
      PrettyEffect("Others (excl. PvP)", plugin.BattleEffectsConfigurator.BattleEffectOther);
      ImGui.Unindent();
    }

    private bool DrawTableGroup(ref ConfigForGroupingSize config)
    {
      var changed = false;
      var self = config.Self;
      var party = config.Party;
      var other = config.Other;
      var onlyInDuty = config.OnlyInDuty;

      ImGui.PushID($"{config.Size}");

      ImGui.BeginTable("Table", 4);


      if (DrawRadiosLine($"Own", ref self))
      {
        config.Self = self;
        changed = true;
      }
      if (DrawRadiosLine($"Party", ref party))
      {
        config.Party = party;
        changed = true;
      }
      if (DrawRadiosLine($"Others (excl. PvP)", ref other))
      {
        config.Other = other;
        changed = true;
      }
      if (config.Size != GroupingSize.Backup && config.Size != GroupingSize.Alliance && DrawOnlyInDutyCheckbox($"Only In Duty", ref onlyInDuty))
      {
        config.OnlyInDuty = onlyInDuty;
        changed = true;
      }

      ImGui.EndTable();

      ImGui.PopID();

      return changed;
    }

    private bool DrawOnlyInDutyCheckbox(string label, ref bool onlyInDuty)
    {
      var changed = false;

      ImGui.TableNextRow();
      ImGui.TableSetColumnIndex(0);
      ImGui.Text(label);

      ImGui.PushID(label);

      ImGui.TableSetColumnIndex(1);
      if (ImGui.Checkbox($"##{label}", ref onlyInDuty))
      {
        changed = true;
      }

      ImGui.PopID();

      return changed;
    }

    private bool DrawRadiosLine(string label, ref BattleEffect effect)
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

    private void DrawGroupingSizeGroup(ConfigForGroupingSize config, bool isActive)
    {
      if (DrawTableGroup(ref config))
      {
        if (isActive)
        {
          plugin.BattleEffectsConfigurator.UIChange(config.Size);
        }
        plugin.Configuration.Save();
      }
    }

    private void DrawPrettyHeader(ConfigForGroupingSize config, bool isActive)
    {

      string headerText = config.Size switch
      {
        GroupingSize.Solo => "Solo",
        GroupingSize.LightParty => "Light Party (4-man)",
        GroupingSize.FullParty => "Full Party (8-man)",
        GroupingSize.Alliance => "Alliance Raids (24-man Duty)",
        _ => "Saved In-Game Settings",
      };

      if (isActive)
      {
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 255, 0, 255));
      }

      if (ImGui.CollapsingHeader(headerText))
      {
        if (isActive)
        {
          ImGui.PopStyleColor();
        }

        ImGui.Indent();

        DrawGroupingSizeGroup(config, isActive);

        ImGui.Unindent();
      }
      else
      {
        if (isActive)
        {
          ImGui.PopStyleColor();
        }
      }
    }

    private void DrawMatrixSection(ConfigForGroupingSize activeConfig)
    {
      DrawPrettyHeader(plugin.Configuration.Solo, plugin.Configuration.Solo == activeConfig && plugin.Configuration.Enabled);
      DrawPrettyHeader(plugin.Configuration.LightParty, plugin.Configuration.LightParty == activeConfig && plugin.Configuration.Enabled);
      DrawPrettyHeader(plugin.Configuration.FullParty, plugin.Configuration.FullParty == activeConfig && plugin.Configuration.Enabled);
      DrawPrettyHeader(plugin.Configuration.Alliance, plugin.Configuration.Alliance == activeConfig && plugin.Configuration.Enabled);
      DrawPrettyHeader(plugin.Configuration.Backup, plugin.Configuration.Backup == activeConfig || !plugin.Configuration.Enabled);
    }

    public void DrawDebugSection()
    {
      if (ImGui.CollapsingHeader("Debug Options"))
      {
        ImGui.Indent();

        ImGui.TextWrapped("Use these to test your settings.");

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

        var forceInDuty = plugin.Configuration.DebugForceInDuty;
        if (ImGui.Checkbox("Force In Duty", ref forceInDuty))
        {
          plugin.Configuration.DebugForceInDuty = forceInDuty;
          plugin.Configuration.Save();
        }

        ImGui.Unindent();
      }
    }

    public override void Draw()
    {
      var groupSize = plugin.BattleEffectsConfigurator.GetCurrentGroupingSize();
      var activeConfig = plugin.Configuration.GetConfigForGroupingSize(groupSize, plugin.BoundByDuty);

      DrawSectionMasterEnable(activeConfig);

      ImGui.Separator();

      DrawMatrixSection(activeConfig);

      ImGui.Separator();

      DrawDebugSection();
    }
  }
}
