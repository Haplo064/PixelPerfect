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
using System.Collections.Generic;
using ImPlotNET;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using System.Xml.Serialization;

namespace PixelPerfect
{
    public partial class PixelPerfect : IDalamudPlugin
    {
        private void DrawEditor()
        {
            if (_editor)
            {
                if (_cs.LocalPlayer == null) return;

                var mX = ImGui.GetMousePos().X;
                var mY = ImGui.GetMousePos().Y;

                ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Pixel Perfect Editor", ref _editor);
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Scale", ref editorScale, 0.1f, 1f);
                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Each box is 1 Yalm by 1 Yalm"); }
                ImGui.PopItemWidth();
                if (editorScale <= 0) editorScale = 0.1f;

                var windowPos = ImGui.GetWindowPos();
                var windowMax = ImGui.GetWindowContentRegionMax();
                
                var linesY = Math.Ceiling(windowMax.Y / (10 * editorScale));
                var linesX = Math.Ceiling(windowMax.X / (10 * editorScale));

                if (ImGui.Button("Help"))
                {
                    _editorHelp = !_editorHelp;
                }

                //Drawing the yalm Grid
                for (int i = 0; i < linesY; i++)
                {
                    ImGui.GetWindowDrawList().AddLine(new Vector2(windowPos.X, windowPos.Y + 100 + (10 * i * editorScale)), new Vector2(windowPos.X + windowMax.X, windowPos.Y + 100 + (10 * i * editorScale)), ImGui.GetColorU32(new Vector4(0.8f, 0.8f, 0.8f, 0.5f)));
                }
                for (int i = 0; i < linesX; i++)
                {
                    ImGui.GetWindowDrawList().AddLine(new Vector2(windowPos.X + (10 * i * editorScale), windowPos.Y + 100), new Vector2(windowPos.X + (10 * i * editorScale), windowPos.Y + windowMax.Y + 100), ImGui.GetColorU32(new Vector4(0.8f, 0.8f, 0.8f, 0.5f)));
                }
                var dotPosX = windowPos.X + (windowMax.X / 2);
                var dotPosY = windowPos.Y + 50 + (windowMax.Y / 2);
                ImGui.GetWindowDrawList().AddCircleFilled(new Vector2(dotPosX, dotPosY), 10f, ImGui.GetColorU32(new Vector4(0.8f, 0.8f, 0.8f, 0.5f)));

                var loop = 0;
                var alpha = 1;
                var skip = false;
                foreach (var doodle in doodleBag)
                {
                    if (!doodle.Enabled)
                    {
                        loop++;
                        continue;
                    }
                    if(!CheckJob(_cs.LocalPlayer.ClassJob.Id, doodle.JobsBool))
                    {
                        loop++;
                        continue;
                    }
                    if (loop == selected)
                    {
                        alpha = 4;
                        if (mX > dotPosX + (doodle.Vector.W * 10 * editorScale) - 20
                            && mX < dotPosX + (doodle.Vector.W * 10 * editorScale) + 20
                            && mY > dotPosY + (doodle.Vector.X * 10 * editorScale) - 20
                            && mY < dotPosY + (doodle.Vector.X * 10 * editorScale) + 20)
                        {
                            if (grabbed == -1 && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !skip)
                            {
                                skip = true;
                                grabbed = 1;
                            }
                            if (grabbed == 1 && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !skip)
                            {
                                skip = true;
                                grabbed = -1;
                            }
                        }
                        if (grabbed == 1)
                        {
                            doodle.Vector = new(
                                (mY - dotPosY) / (10 * editorScale),
                                doodle.Vector.Y,
                                doodle.Vector.Z,
                                (mX - dotPosX) / (10 * editorScale));
                        }

                        if (mX > dotPosX + (doodle.Vector.Y * 10 * editorScale) - 20
                                && mX < dotPosX + (doodle.Vector.Y * 10 * editorScale) + 20
                                && mY > dotPosY + (doodle.Vector.Z * 10 * editorScale) - 20
                                && mY < dotPosY + (doodle.Vector.Z * 10 * editorScale) + 20)
                        {
                            if (grabbed == -1 && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !skip)
                            {
                                skip = true;
                                grabbed = 2;
                            }
                            if (grabbed == 2 && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !skip)
                            {
                                skip = true;
                                grabbed = -1;
                            }
                        }
                        if (grabbed == 2)
                        {
                            doodle.Vector = new(
                                doodle.Vector.X,
                                (mX - dotPosX) / (10 * editorScale),
                                (mY - dotPosY) / (10 * editorScale),
                                doodle.Vector.W);
                        }
                    }
                    else
                    {
                        alpha = 1;
                    }
                    if (doodle.Type == 0)//Ring
                    {
                        DrawRingEditor(dotPosX, dotPosY,
                            doodle.Radius * 10 * editorScale,
                            doodle.Segments,
                            doodle.Thickness,
                            ImGui.GetColorU32(new Vector4(doodle.Colour.X, doodle.Colour.Y, doodle.Colour.Z, doodle.Colour.W * (0.25f * alpha))));
                    }
                    if (doodle.Type == 1)//Line
                    {
                        var x1 = dotPosX + (doodle.Vector.W * 10 * editorScale);
                        var y1 = dotPosY + (doodle.Vector.X * 10 * editorScale);

                        var x2 = dotPosX + (doodle.Vector.Y * 10 * editorScale);
                        var y2 = dotPosY + (doodle.Vector.Z * 10 * editorScale);

                        if (doodle.North)
                        {
                            ImGui.GetWindowDrawList().AddLine(
                                new Vector2(x1, y1),
                                new Vector2(x2, y2),
                                ImGui.GetColorU32(new Vector4(doodle.Colour.X, doodle.Colour.Y, doodle.Colour.Z, doodle.Colour.W * (0.25f * alpha))), doodle.Thickness);
                        }
                        else
                        {
                            var sin = Math.Sin(-_cs.LocalPlayer.Rotation + Math.PI);
                            var cos = Math.Cos(-_cs.LocalPlayer.Rotation + Math.PI);
                            var xr1 = cos * (x1 - dotPosX) - sin * (y1 - dotPosY) + dotPosX;
                            var yr1 = sin * (x1 - dotPosX) + cos * (y1 - dotPosY) + dotPosY;

                            var xr2 = cos * (x2 - dotPosX) - sin * (y2 - dotPosY) + dotPosX;
                            var yr2 = sin * (x2 - dotPosX) + cos * (y2 - dotPosY) + dotPosY;

                            ImGui.GetWindowDrawList().AddLine(
                                new Vector2((float)xr1, (float)yr1),
                                new Vector2((float)xr2, (float)yr2),
                                ImGui.GetColorU32(new Vector4(doodle.Colour.X, doodle.Colour.Y, doodle.Colour.Z, doodle.Colour.W * (0.25f * alpha))), doodle.Thickness);
                        }
                    }
                    if (doodle.Type == 2)//Circle
                    {
                        if (doodle.Filled)
                        {
                            ImGui.GetWindowDrawList().AddCircleFilled(
                                new Vector2(dotPosX, dotPosY),
                                doodle.Radius,
                                ImGui.GetColorU32(new Vector4(doodle.Colour.X, doodle.Colour.Y, doodle.Colour.Z, doodle.Colour.W * (0.25f * alpha))),
                                doodle.Segments);
                        }
                        else
                        {
                            ImGui.GetWindowDrawList().AddCircle(
                                new Vector2(dotPosX, dotPosY),
                                doodle.Radius,
                                ImGui.GetColorU32(new Vector4(doodle.Colour.X, doodle.Colour.Y, doodle.Colour.Z, doodle.Colour.W * (0.25f * alpha))),
                                doodle.Segments, doodle.Thickness);
                        }

                    }
                    loop++;

                }



                ImGui.End();
            }

            if (_editorHelp)
            {
                ImGui.SetNextWindowSize(new Vector2(300, 300), ImGuiCond.FirstUseEver);
                ImGui.Begin("Pixel Perfect Editor Help", ref _editorHelp);
                ImGui.TextWrapped("Here you can see and edit your overlay in real time.");
                ImGui.TextWrapped("Select the Doodle tab in the config, and the selected doodle will highlight in the editor.");
                ImGui.TextWrapped("If you have a `line` doodle, you can also <Left Click> the ends to move them, and <Left Click> again to place them down.");
                ImGui.TextWrapped("The dot in the centre is your player character.");
                ImGui.End();
            }

            if (_update)
            {
                ImGui.SetNextWindowSize(new Vector2(300, 300), ImGuiCond.FirstUseEver);
                ImGui.Begin("Pixel Perfect Update", ref _update);
                ImGui.TextWrapped("- Fixed crash of 0 scale");
                ImGui.TextWrapped("- Added better Job selection");
                ImGui.End();
            }
        }
    }
}
