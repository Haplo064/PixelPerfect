using System;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using ImGuiNET;
using PixelPerfect.Data;

namespace PixelPerfect.Windows;

public class ConfigWindow : IDisposable
{
    private Config _config;
    public bool Visible;

    public void Init(Config config)
    {
        _config = config;
    }

    public void Draw()
    {
        DrawSettingsWindow();
    }

    private void DrawSettingsWindow()
    {
        if (Visible)
        {
            ImGui.SetNextWindowSize(new Vector2(300, 500), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Pixel Perfect Config", ref Visible))
            {
                _config.Save();
            }

            if (ImGui.Checkbox("Combat Only", ref _config.Combat))
            {
                _config.Save();
            }

            ToolTip("Only show all of this during combat");

            ImGui.SameLine();
            if (ImGui.Checkbox("Instance Only", ref _config.Instance))
            {
                _config.Save();
            }

            ToolTip("Only show all of this during instances (like dungeons, raids etc)");

            ImGui.Separator();
            if (ImGui.Checkbox("Hitbox", ref _config.Enabled))
            {
                _config.Save();
            }

            ToolTip("A visual representation of your hitbox");

            if (_config.Enabled)
            {
                ImGui.SameLine();
                if (ImGui.ColorEdit4("Hitbox Colour", ref _config.Col, ImGuiColorEditFlags.NoInputs))
                {
                    _config.Save();
                }

                ToolTip("The colour of the hitbox");
            }

            if (ImGui.Checkbox("Outer Ring", ref _config.Circle))
            {
                _config.Save();
            }

            ToolTip("A tiny circle of colour around the hitbox dot");

            if (_config.Circle)
            {
                ImGui.SameLine();
                if (ImGui.ColorEdit4("Outer Colour", ref _config.Col2, ImGuiColorEditFlags.NoInputs))
                {
                    _config.Save();
                }

                ToolTip("The colour of the ring");
            }

            if (ImGui.Checkbox("Show North", ref _config.North1))
            {
                _config.Save();
            }

            ToolTip("An indicator that always points north");

            if (_config.North1)
            {
                ImGui.SameLine();
                if (ImGui.Checkbox("Line", ref _config.North2))
                {
                    _config.Save();
                }

                ToolTip("What can I say. It's a line");

                ImGui.SameLine();
                if (ImGui.Checkbox("Chevron", ref _config.North3))
                {
                    _config.Save();
                }

                ToolTip("Fancy word for a pointer");

                if (_config.North2)
                {
                    if (ImGui.DragFloat("Line Offset", ref _config.LineOffset))
                    {
                        _config.Save();
                    }

                    ToolTip("How far from your hitbox to start the line");

                    if (ImGui.DragFloat("Line Length", ref _config.LineLength))
                    {
                        _config.Save();
                    }

                    ToolTip("How long the line is");

                    if (ImGui.DragFloat("Line Thickness", ref _config.LineThicc))
                    {
                        _config.Save();
                    }

                    ToolTip("How thicc the line is");

                    if (ImGui.ColorEdit4("Line Colour", ref _config.LineCol, ImGuiColorEditFlags.NoInputs))
                    {
                        _config.Save();
                    }

                    ToolTip("The colour of the line");
                }

                if (_config.North3)
                {
                    if (ImGui.DragFloat("Chevron Offset", ref _config.ChevOffset))
                    {
                        _config.Save();
                    }

                    ToolTip("These are all a bit iffy, due to maths. Mess around with them");

                    if (ImGui.DragFloat("Chevron Length", ref _config.ChevLength))
                    {
                        _config.Save();
                    }

                    ToolTip("These are all a bit iffy, due to maths. Mess around with them");

                    if (ImGui.DragFloat("Chevron Radius", ref _config.ChevRad))
                    {
                        _config.Save();
                    }

                    ToolTip("These are all a bit iffy, due to maths. Mess around with them");

                    if (ImGui.DragFloat("Chevron Sin", ref _config.ChevSin))
                    {
                        _config.Save();
                    }

                    ToolTip("These are all a bit iffy, due to maths. Mess around with them");

                    if (ImGui.DragFloat("Chevron Thickness", ref _config.ChevThicc))
                    {
                        _config.Save();
                    }

                    ToolTip("How thicc the Chevron is");

                    if (ImGui.ColorEdit4("Chevron Colour", ref _config.ChevCol, ImGuiColorEditFlags.NoInputs))
                    {
                        _config.Save();
                    }

                    ToolTip("The colour of the Chevron");
                }
            }


            ImGui.Separator();
            if (ImGui.Checkbox("Ring", ref _config.Ring))
            {
                _config.Save();
            }

            ToolTip("Show a ring around your character");

            if (_config.Ring)
            {
                if (ImGui.DragFloat("Yalms", ref _config.Radius))
                {
                    _config.Save();
                }

                ToolTip("The radius of the ring");

                if (ImGui.DragFloat("Thickness", ref _config.Thickness))
                {
                    _config.Save();
                }

                ToolTip("The thiccness of the ring");

                if (ImGui.DragInt("Smoothness", ref _config.Segments))
                {
                    _config.Save();
                }

                ToolTip("How many segments to make the ring out of");

                if (ImGui.ColorEdit4("Ring Colour", ref _config.ColRing, ImGuiColorEditFlags.NoInputs))
                {
                    _config.Save();
                }

                ToolTip("The colour of the ring");
            }

            ImGui.Separator();
            if (ImGui.Checkbox("Ring 2", ref _config.Ring2))
            {
                _config.Save();
            }

            ToolTip("Show another ring around your character");

            if (_config.Ring2)
            {
                if (ImGui.DragFloat("Yalms 2", ref _config.Radius2))
                {
                    _config.Save();
                }

                ToolTip("The radius of the ring");

                if (ImGui.DragFloat("Thickness 2", ref _config.Thickness2))
                {
                    _config.Save();
                }

                ToolTip("The thiccness of the ring");

                if (ImGui.DragInt("Smoothness 2", ref _config.Segments2))
                {
                    _config.Save();
                }

                ToolTip("How many segments to make the ring out of");

                if (ImGui.ColorEdit4("Ring Colour 2", ref _config.ColRing2, ImGuiColorEditFlags.NoInputs))
                {
                    _config.Save();
                }

                ToolTip("The colour of the ring");
            }

            if (ImGui.Button("Save and Close Config"))
            {
                _config.Save();
                Visible = false;
            }

            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);

            if (ImGui.Button("Buy Haplo a Hot Chocolate"))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://ko-fi.com/haplo",
                    UseShellExecute = true
                });
            }

            ImGui.PopStyleColor(3);
            ImGui.End();
        }

        if (Service.ClientState.LocalPlayer == null) return;

        if (_config.Combat)
        {
            if (!Service.Condition[ConditionFlag.InCombat])
            {
                return;
            }
        }

        if (_config.Instance)
        {
            if (!Service.Condition[ConditionFlag.BoundByDuty])
            {
                return;
            }
        }

        var actor = Service.ClientState.LocalPlayer;
        if (!Service.GameGui.WorldToScreen(
                new Vector3(actor.Position.X, actor.Position.Y, actor.Position.Z),
                out var pos)) return;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGuiHelpers.ForceNextWindowMainViewport();
        ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(0, 0));
        ImGui.Begin("Ring",
            ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
        ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

        if (_config.Enabled)
        {
            ImGui.GetWindowDrawList().AddCircleFilled(
                new Vector2(pos.X, pos.Y),
                2f,
                ImGui.GetColorU32(_config.Col),
                100);
        }

        if (_config.Circle)
        {
            ImGui.GetWindowDrawList().AddCircle(
                new Vector2(pos.X, pos.Y),
                2.2f,
                ImGui.GetColorU32(_config.Col2),
                100);
        }

        if (_config.Ring)
        {
            DrawRingWorld(Service.ClientState.LocalPlayer, _config.Radius, _config.Segments, _config.Thickness,
                ImGui.GetColorU32(_config.ColRing));
        }

        if (_config.Ring2)
        {
            DrawRingWorld(Service.ClientState.LocalPlayer, _config.Radius2, _config.Segments2, _config.Thickness2,
                ImGui.GetColorU32(_config.ColRing2));
        }

        if (_config.North1)
        {
            //Tip of arrow
            Service.GameGui.WorldToScreen(new Vector3(
                    actor.Position.X + ((_config.LineLength + _config.LineOffset) * (float)Math.Sin(Math.PI)),
                    actor.Position.Y,
                    actor.Position.Z + ((_config.LineLength + _config.LineOffset) * (float)Math.Cos(Math.PI))
                ),
                out var lineTip);
            //Player + offset
            Service.GameGui.WorldToScreen(new Vector3(
                    actor.Position.X + (_config.LineOffset * (float)Math.Sin(Math.PI)),
                    actor.Position.Y,
                    actor.Position.Z + (_config.LineOffset * (float)Math.Cos(Math.PI))
                ),
                out var lineOffset);
            //Chev offset1
            Service.GameGui.WorldToScreen(new Vector3(
                    actor.Position.X + (_config.ChevOffset * (float)Math.Sin(Math.PI / _config.ChevRad) *
                                        _config.ChevSin),
                    actor.Position.Y,
                    actor.Position.Z + (_config.ChevOffset * (float)Math.Cos(Math.PI / _config.ChevRad) *
                                        _config.ChevSin)
                ),
                out var chevOffset1);
            //Chev offset2
            Service.GameGui.WorldToScreen(new Vector3(
                    actor.Position.X + (_config.ChevOffset * (float)Math.Sin(Math.PI / -_config.ChevRad) *
                                        _config.ChevSin),
                    actor.Position.Y,
                    actor.Position.Z + (_config.ChevOffset * (float)Math.Cos(Math.PI / -_config.ChevRad) *
                                        _config.ChevSin)
                ),
                out var chevOffset2);
            //Chev Tip
            Service.GameGui.WorldToScreen(new Vector3(
                    actor.Position.X + ((_config.ChevOffset + _config.ChevLength) * (float)Math.Sin(Math.PI)),
                    actor.Position.Y,
                    actor.Position.Z + ((_config.ChevOffset + _config.ChevLength) * (float)Math.Cos(Math.PI))
                ),
                out var chevTip);
            if (_config.North2)
            {
                ImGui.GetWindowDrawList().AddLine(new Vector2(lineTip.X, lineTip.Y),
                    new Vector2(lineOffset.X, lineOffset.Y),
                    ImGui.GetColorU32(_config.LineCol), _config.LineThicc);
            }

            if (_config.North3)
            {
                ImGui.GetWindowDrawList().AddLine(new Vector2(chevTip.X, chevTip.Y),
                    new Vector2(chevOffset1.X, chevOffset1.Y),
                    ImGui.GetColorU32(_config.ChevCol), _config.ChevThicc);
                ImGui.GetWindowDrawList().AddLine(new Vector2(chevTip.X, chevTip.Y),
                    new Vector2(chevOffset2.X, chevOffset2.Y),
                    ImGui.GetColorU32(_config.ChevCol), _config.ChevThicc);
            }
        }

        ImGui.End();
        ImGui.PopStyleVar();
    }

    private void ToolTip(string hoverText)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(hoverText);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private static void DrawRingWorld(GameObject actor, float radius, int numSegments, float thicc, uint colour)
    {
        var seg = numSegments / 2;
        for (var i = 0; i <= numSegments; i++)
        {
            Service.GameGui.WorldToScreen(new Vector3(
                    actor.Position.X + (radius * (float)Math.Sin((Math.PI / seg) * i)),
                    actor.Position.Y,
                    actor.Position.Z + (radius * (float)Math.Cos((Math.PI / seg) * i))
                ),
                out var pos);
            ImGui.GetWindowDrawList().PathLineTo(new Vector2(pos.X, pos.Y));
        }

        ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.None, thicc);
    }
}