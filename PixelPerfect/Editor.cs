using System;
using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace PixelPerfect
{
    public partial class PixelPerfect
    {
        private void DrawEditor()
        {
            if (_editor)
            {
                if (_ot.LocalPlayer == null) return;

                var mX = ImGui.GetMousePos().X;
                var mY = ImGui.GetMousePos().Y;

                ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Pixel Perfect Editor", ref _editor);
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("Scale", ref _editorScale, 0.1f, 1f);
                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Each box is 1 Yalm by 1 Yalm"); }
                ImGui.PopItemWidth();
                if (_editorScale <= 0.1f) _editorScale = 0.1f;

                var windowPos = ImGui.GetWindowPos();
                var windowMax = ImGui.GetWindowContentRegionMax();
                
                var linesY = Math.Ceiling(windowMax.Y / (10 * _editorScale));
                var linesX = Math.Ceiling(windowMax.X / (10 * _editorScale));

                if (ImGui.Button("Help"))
                {
                    _editorHelp = !_editorHelp;
                }

                //Drawing the yalm Grid
                for (int i = 0; i < linesY; i++)
                {
                    ImGui.GetWindowDrawList().AddLine(windowPos with { Y = windowPos.Y + 100 + (10 * i * _editorScale) }, new Vector2(windowPos.X + windowMax.X, windowPos.Y + 100 + (10 * i * _editorScale)), ImGui.GetColorU32(new Vector4(0.8f, 0.8f, 0.8f, 0.5f)));
                }
                for (int i = 0; i < linesX; i++)
                {
                    ImGui.GetWindowDrawList().AddLine(new Vector2(windowPos.X + (10 * i * _editorScale), windowPos.Y + 100), new Vector2(windowPos.X + (10 * i * _editorScale), windowPos.Y + windowMax.Y + 100), ImGui.GetColorU32(new Vector4(0.8f, 0.8f, 0.8f, 0.5f)));
                }
                var anchorX = windowPos.X + (windowMax.X / 2);
                var anchorY = windowPos.Y + 50 + (windowMax.Y / 2);
                ImGui.GetWindowDrawList().AddCircleFilled(new Vector2(anchorX, anchorY), 10f, ImGui.GetColorU32(new Vector4(0.8f, 0.8f, 0.8f, 0.5f)));

                var loop = 0;
                var skip = false;
                foreach (var doodle in _doodleBag)
                {
                    if (!doodle.Enabled)
                    {
                        loop++;
                        continue;
                    }
                    if(!CheckJob(_ot.LocalPlayer.ClassJob.RowId, doodle.JobsBool))
                    {
                        loop++;
                        continue;
                    }

                    var drawX = anchorX;
                    var drawY = anchorY;

                    int alpha;
                    if (loop == _selected)
                    {
                        alpha = 4;
                        if (mX > drawX + (doodle.Vector.W * 10 * _editorScale) - 20
                            && mX < drawX + (doodle.Vector.W * 10 * _editorScale) + 20
                            && mY > drawY + (doodle.Vector.X * 10 * _editorScale) - 20
                            && mY < drawY + (doodle.Vector.X * 10 * _editorScale) + 20)
                        {
                            if (_grabbed == -1 && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !skip)
                            {
                                skip = true;
                                _grabbed = 1;
                            }
                            if (_grabbed == 1 && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !skip)
                            {
                                skip = true;
                                _grabbed = -1;
                                _dirty = true;
                            }
                        }
                        if (_grabbed == 1)
                        {
                            doodle.Vector = doodle.Vector with { X = (mY - drawY) / (10 * _editorScale), W = (mX - drawX) / (10 * _editorScale) };
                        }

                        if (mX > drawX + (doodle.Vector.Y * 10 * _editorScale) - 20
                                && mX < drawX + (doodle.Vector.Y * 10 * _editorScale) + 20
                                && mY > drawY + (doodle.Vector.Z * 10 * _editorScale) - 20
                                && mY < drawY + (doodle.Vector.Z * 10 * _editorScale) + 20)
                        {
                            if (_grabbed == -1 && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !skip)
                            {
                                skip = true;
                                _grabbed = 2;
                            }
                            if (_grabbed == 2 && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !skip)
                            {
                                skip = true;
                                _grabbed = -1;
                                _dirty = true;
                            }
                        }
                        if (_grabbed == 2)
                        {
                            doodle.Vector = doodle.Vector with { Y = (mX - drawX) / (10 * _editorScale), Z = (mY - drawY) / (10 * _editorScale) };
                        }
                    }
                    else
                    {
                        alpha = 1;
                    }
                    if (doodle.Type == 0)//Ring
                    {
                        if (doodle.Offset && !doodle.RotateOffset)
                        {
                            drawX += (doodle.Vector.X * 10 * _editorScale);
                            drawY += (doodle.Vector.Y * 10 * _editorScale);
                        }
                        
                        if (doodle.RotateOffset)
                        {
                            var angle = -_ot.LocalPlayer.Rotation;
                            var cosTheta = MathF.Cos(angle);
                            var sinTheta = MathF.Sin(angle);
                            drawX += (cosTheta * (doodle.Vector.X * 10 * _editorScale) - sinTheta * (doodle.Vector.Y * 10 * _editorScale));
                            drawY += (sinTheta * (doodle.Vector.X * 10 * _editorScale) + cosTheta * (doodle.Vector.Y * 10 * _editorScale));
                        }
                        DrawRingEditor(drawX, drawY,
                            doodle.Radius * 10 * _editorScale,
                            doodle.Segments,
                            doodle.Thickness,
                            ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) }));
                    }
                    if (doodle.Type == 1)//Line
                    {
                        var x1 = drawX + (doodle.Vector.W * 10 * _editorScale);
                        var y1 = drawY + (doodle.Vector.X * 10 * _editorScale);

                        var x2 = drawX + (doodle.Vector.Y * 10 * _editorScale);
                        var y2 = drawY + (doodle.Vector.Z * 10 * _editorScale);

                        if (doodle.North)
                        {

                            ImGui.GetWindowDrawList().AddLine(
                                new Vector2(x1, y1),
                                new Vector2(x2, y2),
                                ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) }), doodle.Thickness);
                        }
                        else
                        {
                            var sin = Math.Sin(-_ot.LocalPlayer.Rotation + Math.PI);
                            var cos = Math.Cos(-_ot.LocalPlayer.Rotation + Math.PI);
                            var xr1 = cos * (x1 - drawX) - sin * (y1 - drawY) + drawX;
                            var yr1 = sin * (x1 - drawX) + cos * (y1 - drawY) + drawY;

                            var xr2 = cos * (x2 - drawX) - sin * (y2 - drawY) + drawX;
                            var yr2 = sin * (x2 - drawX) + cos * (y2 - drawY) + drawY;

                            ImGui.GetWindowDrawList().AddLine(
                                new Vector2((float)xr1, (float)yr1),
                                new Vector2((float)xr2, (float)yr2),
                                ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) }), doodle.Thickness);
                        }
                    }
                    if (doodle.Type == 2)//Dot
                    {
                        if (doodle.Offset)
                        {
                            drawX += (doodle.Vector.X * 10 * _editorScale);
                            drawY += (doodle.Vector.Y * 10 * _editorScale);
                        }

                        if (doodle.North)
                        {
                            if (doodle.Outline)
                            {
                                ImGui.GetWindowDrawList().AddCircle(
                                     new Vector2(drawX, drawY),
                                    doodle.Radius + doodle.Thickness * 0.6f,
                                    ImGui.GetColorU32(doodle.OutlineColour with { W = doodle.OutlineColour.W * (0.25f * alpha) }),
                                    doodle.Segments, doodle.Thickness);
                            }
                            if (doodle.Filled)
                            {
                                ImGui.GetWindowDrawList().AddCircleFilled(
                                    new Vector2(drawX, drawY),
                                    doodle.Radius,
                                    ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) }),
                                    doodle.Segments);
                            }
                            else
                            {
                                ImGui.GetWindowDrawList().AddCircle(
                                    new Vector2(drawX, drawY),
                                    doodle.Radius,
                                    ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) }),
                                    doodle.Segments, doodle.Thickness);
                            }
                        }
                        else
                        {
                            var x1 = drawX + (doodle.Vector.W * 10 * _editorScale);
                            var y1 = drawY + (doodle.Vector.X * 10 * _editorScale);

                            var sin = Math.Sin(-_ot.LocalPlayer.Rotation + Math.PI);
                            var cos = Math.Cos(-_ot.LocalPlayer.Rotation + Math.PI);
                            var xr1 = cos * (x1 - drawX) - sin * (y1 - drawY) + drawX;
                            var yr1 = sin * (x1 - drawX) + cos * (y1 - drawY) + drawY;

                            if (doodle.Outline)
                            {
                                ImGui.GetWindowDrawList().AddCircle(
                                     new Vector2((float)xr1, (float)yr1),
                                    doodle.Radius + doodle.Thickness * 0.6f,
                                    ImGui.GetColorU32(doodle.OutlineColour with { W = doodle.OutlineColour.W * (0.25f * alpha) }),
                                    doodle.Segments, doodle.Thickness);
                            }
                            if (doodle.Filled)
                            {
                                ImGui.GetWindowDrawList().AddCircleFilled(
                                    new Vector2((float)xr1, (float)yr1),
                                    doodle.Radius,
                                    ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) }),
                                    doodle.Segments);
                            }
                            else
                            {
                                ImGui.GetWindowDrawList().AddCircle(
                                    new Vector2((float)xr1, (float)yr1),
                                    doodle.Radius,
                                    ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) }),
                                    doodle.Segments, doodle.Thickness);
                            }
                        }
                    }
                    if (doodle.Type == 3)//Dashed ring
                    {
                        if (doodle.Offset && !doodle.RotateOffset)
                        {
                            drawX += (doodle.Vector.X * 10 * _editorScale);
                            drawY += (doodle.Vector.Y * 10 * _editorScale);
                        }
                        if (doodle.RotateOffset)
                        {
                            var angle = -_ot.LocalPlayer.Rotation;
                            var cosTheta = MathF.Cos(angle);
                            var sinTheta = MathF.Sin(angle);
                            drawX += (cosTheta * (doodle.Vector.X * 10 * _editorScale) - sinTheta * (doodle.Vector.Y * 10 * _editorScale));
                            drawY += (sinTheta * (doodle.Vector.X * 10 * _editorScale) + cosTheta * (doodle.Vector.Y * 10 * _editorScale));
                        }
                        float segAng = MathF.Tau / doodle.Segments;
                        uint col = ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) });
                        for (int i = 0; i < doodle.Segments; i++)
                        {
                            Vector2 pos1 = new Vector2(
                                drawX + doodle.Radius * 10 * _editorScale * MathF.Sin(segAng * i),
                                drawY + doodle.Radius * 10 * _editorScale * MathF.Cos(segAng * i));
                            Vector2 pos2 = new Vector2(
                                drawX + doodle.Radius * 10 * _editorScale * MathF.Sin(segAng * (i + 0.4f)),
                                drawY + doodle.Radius * 10 * _editorScale * MathF.Cos(segAng * (i + 0.4f)));
                            ImGui.GetWindowDrawList().AddLine(pos1, pos2, col, doodle.Thickness);
                        }
                    }
                    if (doodle.Type == 4)//Cone
                    {
                    //Not used
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
                ImGui.SetNextWindowSize(new Vector2(300, 400), ImGuiCond.FirstUseEver);
                ImGui.Begin("Pixel Perfect Update", ref _update);
                ImGui.TextWrapped("Updated for 7.0!");
                ImGui.TextWrapped("- Added VPR and PCT.");
                ImGui.TextWrapped("- Added Cones for object drawing");
                ImGui.TextWrapped("- Added Z Editing");
                if (ImGui.Button("Open Config"))
                {
                    _config = true;
                }
                ImGui.End();
            }
        }
    }
}
