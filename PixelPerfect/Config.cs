﻿using System;
using System.Diagnostics;
using ImGuiNET;
using System.Numerics;
using System.Collections.Generic;
using System.Text;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;


namespace PixelPerfect;

public partial class PixelPerfect
{
    private void addNotification(string content, NotificationType type)
    {
        Notification notif = new Notification();
        notif.Content = content;
        notif.Type = type;
        _nm.AddNotification(notif);
    }

    private void DrawConfig()
    {
        if (_firstTime && !_bitch)
        {
            ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
            ImGui.Begin("Welcome to Pixel Perfect!", ref _firstTime);
            ImGui.TextWrapped("Hey, and thanks for installing my plugin!");
            ImGui.Text("");
            ImGui.TextWrapped("Use the config menu, and add a doodle to get started.");
            if (ImGui.Button("Open Config"))
            {
                _config = true;
            }
        }

        var deleteNum = -1;
        var moveNum = -1;
        var moveUp = false;

        if (_config)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(750, 650));
            ImGui.SetNextWindowSize(new Vector2(750, 650), ImGuiCond.FirstUseEver);
            ImGui.Begin("Pixel Perfect Config", ref _config);

            ImGui.BeginTabBar("Config Tabs");

            if (ImGui.BeginTabItem("Config##Doodles"))
            {
                var number2 = 0;
                ImGui.Checkbox("Hide Updates", ref _bitch);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Never show any messages.");
                }

                ImGui.Separator();
                foreach (var doodle in _doodleBag)
                {
                    var enabled = doodle.Enabled;
                    var combat = doodle.Combat;
                    var instance = doodle.Instance;
                    var unsheathed = doodle.Unsheathed;

                    var name = doodle.Name;
                    ImGui.Checkbox($"Enable ##{number2}", ref enabled);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Turn the doodle on/off entirely");
                    }

                    ImGui.SameLine();
                    ImGui.Checkbox($"Combat ##{number2}", ref combat);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Only show when engaged in combat");
                    }

                    ImGui.SameLine();
                    ImGui.Checkbox($"Instance ##{number2}", ref instance);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Only show when in an instance (a dungeon/raid etc)");
                    }

                    ImGui.SameLine();
                    ImGui.Checkbox($"Unsheathed ##{number2}", ref unsheathed);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Only show when your weapon is unsheathed");
                    }

                    ImGui.SameLine();
                    ImGui.PushItemWidth(150);
                    ImGui.InputText($"Name##{number2}", ref name, 20);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Name the doodle!");
                    }

                    ImGui.PopItemWidth();
                    ImGui.SameLine();
                    if (number2 > 0)
                    {
                        if (ImGui.Button($"↑##{number2}"))
                        {
                            moveNum = number2;
                            moveUp = true;
                        }

                        ImGui.SameLine();
                    }

                    if (number2 + 1 < _doodleBag.Count)
                    {
                        if (ImGui.Button($"↓##{number2}"))
                        {
                            moveNum = number2;
                        }

                        ImGui.SameLine();
                    }

                    if (ImGui.Button($"Delete##{number2}"))
                    {
                        deleteNum = number2;
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Delete the doodle");
                    }

                    number2++;
                    doodle.Enabled = enabled;
                    doodle.Unsheathed = unsheathed;
                    doodle.Instance = instance;
                    doodle.Combat = combat;
                    doodle.Name = name;
                }

                ImGui.Separator();
                if (ImGui.Button("Add Doodle"))
                {
                    _doodleBag.Add(new Drawing());
                }

                if (ImGui.Button("Show Editor"))
                {
                    _editor = !_editor;
                }

                ImGui.Separator();
                ImGui.TextWrapped("You can export and import your doodles to share, by using the buttons below.");
                ImGui.TextWrapped(
                    "Either export your current doodles to your clipboard, and then share the string with your friends, or import a currently copied exported string into your own doodles, by using the import button!");
                if (ImGui.Button("Export"))
                {
                    var json = JsonConvert.SerializeObject(this._doodleBag);
                    var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
                    ImGui.SetClipboardText(base64);
                    addNotification("Copied to clipboard", NotificationType.Info);
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Exports your current Doodles to your clipboard for sharing!");
                }

                ImGui.SameLine();

                if (ImGui.Button("Import from Clipboard"))
                {
                    try
                    {
                        var base64 = ImGui.GetClipboardText();
                        var jsonBytes = Convert.FromBase64String(base64);
                        var json = Encoding.UTF8.GetString(jsonBytes);
                        var bag = JsonConvert.DeserializeObject<List<Drawing>>(json);
                        _doodleBag.AddRange(bag);
                        SaveConfig();
                        addNotification("Imported successfully", NotificationType.Success);
                    }
                    catch (Exception ex)
                    {
                        addNotification("Could not import", NotificationType.Error);
                    }
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Imports your current clipboard to your Doodles!");
                }

                ImGui.EndTabItem();
            }

            var number = 0;
            _selected = -1;
            foreach (var doodle in _doodleBag)
            {
                if (ImGui.BeginTabItem($"{doodle.Name}##{number}"))
                {
                    _selected = number;

                    var type = doodle.Type;
                    var colour = doodle.Colour;
                    var north = doodle.North;
                    var thickness = doodle.Thickness;
                    var segments = doodle.Segments;
                    var vector = doodle.Vector;
                    var filled = doodle.Filled;
                    var x1 = doodle.Vector.X;
                    var z1 = doodle.Vector.Y;
                    var x2 = doodle.Vector.Z;
                    var z2 = doodle.Vector.W;
                    var radius = doodle.Radius;
                    var job = doodle.Job;
                    var jobsBool = doodle.JobsBool;
                    var offset = doodle.Offset;
                    var rotateOffset = doodle.RotateOffset;
                    var outline = doodle.Outline;
                    var outlineColour = doodle.OutlineColour;

                    ImGui.PushItemWidth(300);
                    ImGui.Combo($"Type ##{number}", ref type, _doodleOptions, _doodleOptions.Length);
                    ImGui.ColorEdit4($"Colour ##{number}", ref colour, ImGuiColorEditFlags.NoInputs);
                    if (ImGui.TreeNode($"Jobs##{number}"))
                    {
                        var loop = 0;
                        ImGui.Columns(6);
                        foreach (var jobb in doodle.JobsBool)
                        {
                            ImGui.Checkbox($"{_doodleJobs[loop]}", ref jobsBool[loop]);

                            if (loop == 0 | loop == 4 | loop == 8 | loop == 13 | loop == 16)
                            {
                                ImGui.NextColumn();
                            }

                            loop++;
                        }

                        ImGui.Columns(1);
                        ImGui.TreePop();
                    }

                    ImGui.InputFloat($"Thickness ##{number}", ref thickness, 0.1f, 1f);

                    if (type == 0) //ring
                    {
                        ImGui.InputFloat($"Radius##{number}", ref radius, 0.1f, 1f);
                        ImGui.InputInt($"Segments ##{number}", ref segments, 1, 10);
                        ImGui.Checkbox($"Offset##{number}", ref offset);
                        if (offset)
                        {
                            ImGui.SameLine();
                            ImGui.Checkbox($"Rotate##{number}", ref rotateOffset);
                            ImGui.InputFloat($"Offset X##{number}", ref x1, 0.1f, 1f);
                            ImGui.InputFloat($"Offset Y##{number}", ref z1, 0.1f, 1f);
                        }
                    }

                    if (type == 1) //line
                    {
                        ImGui.Checkbox($"Locked North ##{number}", ref north);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Otherwise, player relative");
                        }

                        ImGui.PushItemWidth(100);
                        ImGui.InputFloat($"X 1##{number}", ref x1, 0.1f, 1f);
                        ImGui.SameLine();
                        ImGui.InputFloat($"Y 1##{number}", ref z1, 0.1f, 1f);
                        ImGui.InputFloat($"X 2##{number}", ref x2, 0.1f, 1f);
                        ImGui.SameLine();
                        ImGui.InputFloat($"Y 2##{number}", ref z2, 0.1f, 1f);
                        ImGui.PopItemWidth();
                    }

                    if (type == 2) //dot
                    {
                        ImGui.InputFloat($"Radius##{number}", ref radius, 0.1f, 1f);
                        ImGui.InputInt($"Segments ##{number}", ref segments, 1, 10);
                        ImGui.Checkbox($"Filled##{number}", ref filled);
                        ImGui.SameLine();
                        ImGui.Checkbox($"Offset##{number}", ref offset);
                        ImGui.SameLine();
                        ImGui.Checkbox($"Outline##{number}", ref outline);
                        if (outline)
                        {
                            ImGui.ColorEdit4($"Outline Colour ##{number}", ref outlineColour,
                                ImGuiColorEditFlags.NoInputs);
                        }

                        ImGui.Checkbox($"Locked North ##{number}", ref north);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Otherwise, player relative");
                        }

                        if (offset)
                        {
                            ImGui.Checkbox($"Rotate offset relative to player##{number}", ref rotateOffset);
                            ImGui.InputFloat($"Offset X##{number}", ref x1, 0.1f, 1f);
                            ImGui.InputFloat($"Offset Y##{number}", ref z1, 0.1f, 1f);
                        }

                        if (!north)
                        {
                            ImGui.InputFloat($"Offset X2##{number}", ref x2, 0.1f, 1f);
                            ImGui.InputFloat($"Offset Y2##{number}", ref z2, 0.1f, 1f);
                        }
                    }

                    if (type == 3) //dashed ring
                    {
                        ImGui.InputFloat($"Radius##{number}", ref radius, 0.1f, 1f);
                        ImGui.InputInt($"Segments ##{number}", ref segments, 1, 10);
                        ImGui.Checkbox($"Offset##{number}", ref offset);
                        if (offset)
                        {
                            ImGui.SameLine();
                            ImGui.Checkbox($"Rotate##{number}", ref rotateOffset);
                            ImGui.InputFloat($"Offset X##{number}", ref x1, 0.1f, 1f);
                            ImGui.InputFloat($"Offset Y##{number}", ref z1, 0.1f, 1f);
                        }
                    }

                    ImGui.PopItemWidth();
                    doodle.Type = type;
                    doodle.Colour = colour;
                    doodle.North = north;
                    if (thickness < 0f)
                    {
                        thickness = 0f;
                    }

                    doodle.Thickness = thickness;
                    if (segments > 1000)
                    {
                        segments = 1000;
                    }

                    if (segments < 4)
                    {
                        segments = 4;
                    }

                    doodle.Segments = segments;
                    doodle.Vector = vector;
                    doodle.Filled = filled;
                    doodle.Radius = radius;
                    doodle.Vector = new Vector4(x1, z1, x2, z2);
                    doodle.Job = job;
                    doodle.JobsBool = jobsBool;
                    doodle.Offset = offset;
                    doodle.RotateOffset = rotateOffset;
                    doodle.Outline = outline;
                    doodle.OutlineColour = outlineColour;

                    if (ImGui.Button($"Show Editor##{number}"))
                    {
                        _editor = !_editor;
                    }

                    ImGui.EndTabItem();
                }

                number++;
            }

            ImGui.EndTabBar();

            ImGui.Separator();

            if (ImGui.Button("Close"))
            {
                SaveConfig();
                _config = false;
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
            ImGui.PopStyleVar();
            ImGui.End();

            if (_dirtyHack > 100)
            {
                SaveConfig();
                _dirtyHack = 0;
            }

            _dirtyHack++;
            if (deleteNum != -1)
            {
                _doodleBag.RemoveAt(deleteNum);
            }

            if (moveNum == -1) return;
            var doodleA = _doodleBag[moveNum];
            _doodleBag.RemoveAt(moveNum);
            if (moveUp)
            {
                _doodleBag.Insert(moveNum - 1, doodleA);
            }
            else
            {
                _doodleBag.Insert(moveNum + 1, doodleA);
            }
        }
    }
}