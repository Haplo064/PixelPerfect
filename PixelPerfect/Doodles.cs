using System;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface;
using ImGuiNET;
using System.Numerics;
using Dalamud.Interface.Utility;

namespace PixelPerfect
{
    public partial class PixelPerfect
    {
        private void DrawDoodles()
        {
            if (_cs.LocalPlayer == null) return;

            var actor = _cs.LocalPlayer;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(0, 0));
            ImGui.Begin("Canvas",
                ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoFocusOnAppearing);
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

            foreach (var doodle in _doodleBag)
            {
                if (!doodle.Enabled) continue;
                
                if (_condition[ConditionFlag.Occupied38]) continue; // is in-combat cutscene

                if (!CheckJob(_cs.LocalPlayer.ClassJob.RowId, doodle.JobsBool)) continue;
                
                if (doodle.Combat && !_condition[ConditionFlag.InCombat]) continue;

                if (doodle.Instance && !_condition[ConditionFlag.BoundByDuty]) continue;

                if (doodle.Unsheathed && !UIStateHelper.IsWeaponUnsheathed()) continue;

                if (doodle.Type == 0)//Ring
                {
                    float zed = 0f;
                    if (doodle.Zedding)
                    {
                        zed = doodle.Zed;
                    }
                    DrawRingWorld(_cs.LocalPlayer, doodle.Radius, doodle.Segments, doodle.Thickness, ImGui.GetColorU32(doodle.Colour),doodle.Offset, doodle.RotateOffset, doodle.Vector, doodle.Filled, zed);
                }
                if (doodle.Type == 1)//Line
                {
                    float zed = 0f;
                    if (doodle.Zedding)
                    {
                        zed = doodle.Zed;
                    }
                    if (doodle.North)
                    {
                        //Get LinePos1
                        _gui.WorldToScreen(new Vector3(
                            actor.Position.X + doodle.Vector.W,//X1
                            actor.Position.Y+zed,
                            actor.Position.Z + doodle.Vector.X//Y1
                            ), out Vector2 linePos1);

                        //Get LinePos2
                        _gui.WorldToScreen(new Vector3(
                            actor.Position.X + doodle.Vector.Y,//X2
                            actor.Position.Y+zed,
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
                            actor.Position.Y+zed,
                            (float)yr1//Y1
                            ), out Vector2 linePos1);

                        //Get LinePos2
                        _gui.WorldToScreen(new Vector3(
                            (float)xr2,//X2
                            actor.Position.Y+zed,
                            (float)yr2//Y2
                            ), out Vector2 linePos2);

                        ImGui.GetWindowDrawList().AddLine(new Vector2(linePos1.X, linePos1.Y), new Vector2(linePos2.X, linePos2.Y),
                        ImGui.GetColorU32(doodle.Colour), doodle.Thickness);
                    }
                }

                if (doodle.Type == 2)//Dot
                {
                    var xOff = 0f;
                    var yOff = 0f;
                    if (doodle.Offset)
                    {
                        xOff = doodle.Vector.X;
                        yOff = doodle.Vector.Y;
                        if (doodle.RotateOffset)
                        {
                            var angle = -_cs.LocalPlayer.Rotation;
                            var cosTheta = MathF.Cos(angle);
                            var sinTheta = MathF.Sin(angle);
                            xOff = cosTheta * doodle.Vector.X - sinTheta * (doodle.Vector.Y);
                            yOff = sinTheta * doodle.Vector.X + cosTheta * doodle.Vector.Y;
                        }
                    }
                    float zed = 0f;
                    if (doodle.Zedding)
                    {
                        zed = doodle.Zed;
                    }
                    _gui.WorldToScreen(
                         new Vector3(actor.Position.X+xOff, actor.Position.Y+zed, actor.Position.Z+yOff),
                        out var pos);

                    if (doodle.Outline)
                    {
                        ImGui.GetWindowDrawList().AddCircle(
                            new Vector2(pos.X, pos.Y),
                            doodle.Radius + doodle.Thickness * 0.6f,
                            ImGui.GetColorU32(doodle.OutlineColour),
                            doodle.Segments, doodle.Thickness);
                    }

                    if (doodle.Filled)
                    {
                        ImGui.GetWindowDrawList().AddCircleFilled(new Vector2(pos.X, pos.Y), doodle.Radius, ImGui.GetColorU32(doodle.Colour), doodle.Segments);
                    }
                    else
                    {
                        ImGui.GetWindowDrawList().AddCircle(new Vector2(pos.X, pos.Y), doodle.Radius, ImGui.GetColorU32(doodle.Colour), doodle.Segments, doodle.Thickness);
                    }
                }

                if (doodle.Type == 3)//Dashed Ring
                {
                    float zed = 0f;
                    if (doodle.Zedding)
                    {
                        zed = doodle.Zed;
                    }
                    var xOff = 0f;
                    var yOff = 0f;
                    if (doodle.Offset)
                    {
                        xOff = doodle.Vector.X;
                        yOff = doodle.Vector.Y;
                        if (doodle.RotateOffset)
                        {
                            var angle = -actor.Rotation;
                            var cosTheta = MathF.Cos(angle);
                            var sinTheta = MathF.Sin(angle);
                            xOff = cosTheta * doodle.Vector.X - sinTheta * (doodle.Vector.Y);
                            yOff = sinTheta * doodle.Vector.X + cosTheta * doodle.Vector.Y;
                        }
                    }
                    float segAng = MathF.Tau / doodle.Segments;
                    uint col = ImGui.GetColorU32(doodle.Colour);
                    for (int i=0; i<doodle.Segments; i++)
                    {
                        _gui.WorldToScreen(new Vector3(
                            actor.Position.X + xOff + doodle.Radius * MathF.Sin(segAng * i),
                            actor.Position.Y+zed,
                            actor.Position.Z + yOff + doodle.Radius * MathF.Cos(segAng * i)),
                            out var pos1);
                        _gui.WorldToScreen(new Vector3(
                            actor.Position.X + xOff + doodle.Radius * MathF.Sin(segAng * (i + 0.4f)),
                            actor.Position.Y+zed,
                            actor.Position.Z + yOff + doodle.Radius * MathF.Cos(segAng * (i + 0.4f))),
                            out var pos2);
                        ImGui.GetWindowDrawList().AddLine(pos1, pos2, col, doodle.Thickness);
                    }


                }
                if (doodle.Type == 4)//Cone
                {
                    float zed = 0f;
                    if (doodle.Zedding)
                    {
                        zed = doodle.Zed;
                    }
                    DrawConeWorld(_cs.LocalPlayer, doodle.Radius, doodle.Segments, doodle.Thickness, ImGui.GetColorU32(doodle.Colour), doodle.Offset, doodle.RotateOffset, doodle.Vector, doodle.North, doodle.Filled, doodle.Outline,zed);
                }
            }
            ImGui.End();
            ImGui.PopStyleVar();
        }
    }
}
