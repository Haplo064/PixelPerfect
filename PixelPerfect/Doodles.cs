using System;
using System.Diagnostics;
using Dalamud.Configuration;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using System.Numerics;
using System.Collections.Generic;

namespace PixelPerfect
{
    public partial class PixelPerfect : IDalamudPlugin
    {
        private void DrawDoodles()
        {
            if (_cs.LocalPlayer == null) return;

            var actor = _cs.LocalPlayer;

            _gui.WorldToScreen(
                new Vector3(actor.Position.X, actor.Position.Y, actor.Position.Z),
                out var pos);
            
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(0, 0));
            ImGui.Begin("Canvas",
                ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

            foreach (var doodle in doodleBag)
            {
                if (!doodle.Enabled) continue;
                
                if (doodle.Job != 0 && doodleJobsUint[doodle.Job] != _cs.LocalPlayer.ClassJob.Id) continue;

                if (doodle.Combat && !_condition[ConditionFlag.InCombat]) continue;

                if (doodle.Instance && !_condition[ConditionFlag.BoundByDuty]) continue;

                if (doodle.Type == 0)//Ring
                {
                    DrawRingWorld(_cs.LocalPlayer, doodle.Radius, doodle.Segments, doodle.Thickness, ImGui.GetColorU32(doodle.Colour));
                }
                if (doodle.Type == 1)//Line
                {
                    if (doodle.North)
                    {
                        //Get LinePos1
                        _gui.WorldToScreen(new Vector3(
                            actor.Position.X + doodle.Vector.W,//X1
                            actor.Position.Y,
                            actor.Position.Z + doodle.Vector.X//Y1
                            ), out Vector2 linePos1);

                        //Get LinePos2
                        _gui.WorldToScreen(new Vector3(
                            actor.Position.X + doodle.Vector.Y,//X2
                            actor.Position.Y,
                            actor.Position.Z + doodle.Vector.Z//Y2
                            ), out Vector2 linePos2);

                        ImGui.GetWindowDrawList().AddLine(new Vector2(linePos1.X, linePos1.Y), new Vector2(linePos2.X, linePos2.Y),
                        ImGui.GetColorU32(doodle.Colour), doodle.Thickness);
                    }
                    else
                    {
                        var sin = Math.Sin(-_cs.LocalPlayer.Rotation + Math.PI);
                        var cos = Math.Cos(-_cs.LocalPlayer.Rotation + Math.PI);
                        var xr1 = cos * (doodle.Vector.W) - sin * (doodle.Vector.X ) + actor.Position.X;
                        var yr1 = sin * (doodle.Vector.W ) + cos * (doodle.Vector.X) + actor.Position.Z;

                        var xr2 = cos * (doodle.Vector.Y) - sin * (doodle.Vector.Z) + actor.Position.X;
                        var yr2 = sin * (doodle.Vector.Y ) + cos * (doodle.Vector.Z ) + actor.Position.Z;

                        //Get LinePos1
                        _gui.WorldToScreen(new Vector3(
                            (float)xr1,//X1
                            actor.Position.Y,
                            (float)yr1//Y1
                            ), out Vector2 linePos1);

                        //Get LinePos2
                        _gui.WorldToScreen(new Vector3(
                            (float)xr2,//X2
                            actor.Position.Y,
                            (float)yr2//Y2
                            ), out Vector2 linePos2);

                        ImGui.GetWindowDrawList().AddLine(new Vector2(linePos1.X, linePos1.Y), new Vector2(linePos2.X, linePos2.Y),
                        ImGui.GetColorU32(doodle.Colour), doodle.Thickness);
                    }
                }

                if (doodle.Type == 2)//Circle
                {
                    if (doodle.Filled)
                    {
                        ImGui.GetWindowDrawList().AddCircleFilled(new Vector2(pos.X, pos.Y), doodle.Radius, ImGui.GetColorU32(doodle.Colour), doodle.Segments);
                    }
                    else
                    {
                        ImGui.GetWindowDrawList().AddCircle(new Vector2(pos.X, pos.Y), doodle.Radius, ImGui.GetColorU32(doodle.Colour), doodle.Segments, doodle.Thickness);
                    }
                }
            }
            ImGui.End();
            ImGui.PopStyleVar();
        }
    }
}
