using System;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface;
using ImGuiNET;
using System.Numerics;

namespace PixelPerfect
{
    public partial class PixelPerfect
    {
        const float TAU = 6.2831855f;
        private void DrawDoodles()
        {
            if (_cs.LocalPlayer == null) return;

            var actor = _cs.LocalPlayer;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(0, 0));
            ImGui.Begin("Canvas",
                ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

            foreach (var doodle in _doodleBag)
            {
                if (!doodle.Enabled) continue;

                if (!CheckJob(_cs.LocalPlayer.ClassJob.Id, doodle.JobsBool)) continue;
                
                if (doodle.Combat && !_condition[ConditionFlag.InCombat]) continue;

                if (doodle.Instance && !_condition[ConditionFlag.BoundByDuty]) continue;

                if (doodle.Unsheathed && !UIStateHelper.IsWeaponUnsheathed()) continue;

                if (doodle.Type == 0)//Ring
                {
                    DrawRingWorld(_cs.LocalPlayer, doodle.Radius, doodle.Segments, doodle.Thickness, ImGui.GetColorU32(doodle.Colour),doodle.Offset, doodle.RotateOffset, doodle.Vector);
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

                    _gui.WorldToScreen(
                         new Vector3(actor.Position.X+xOff, actor.Position.Y, actor.Position.Z+yOff),
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
                    float segAng = TAU / doodle.Segments;
                    uint col = ImGui.GetColorU32(doodle.Colour);
                    for (int i=0; i<doodle.Segments; i++)
                    {
                        _gui.WorldToScreen(new Vector3(
                            actor.Position.X + xOff + doodle.Radius * MathF.Sin(segAng * i),
                            actor.Position.Y,
                            actor.Position.Z + yOff + doodle.Radius * MathF.Cos(segAng * i)),
                            out var pos1);
                        _gui.WorldToScreen(new Vector3(
                            actor.Position.X + xOff + doodle.Radius * MathF.Sin(segAng * (i + 0.4f)),
                            actor.Position.Y,
                            actor.Position.Z + yOff + doodle.Radius * MathF.Cos(segAng * (i + 0.4f))),
                            out var pos2);
                        ImGui.GetWindowDrawList().AddLine(pos1, pos2, col, doodle.Thickness);
                    }


                }
            }
            ImGui.End();
            ImGui.PopStyleVar();
        }
    }
}
