﻿using System;
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
using System.Text;

namespace PixelPerfect
{
    public partial class PixelPerfect : IDalamudPlugin
    {
        private void DrawConfig()
        {
            if (_firstTime && !_bitch)
            {
                ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Welcome to Pixel Perfect Jump!", ref _firstTime);
                ImGui.TextWrapped("Heya! Thanks for either installing my plugin, or updating it to this new version.");
                ImGui.TextWrapped("Due to the massive changes between this and the previous version, old configs are no longer compatible...");
                ImGui.TextWrapped("However, I have added many long requested features and stuff!");
                ImGui.Text("");
                ImGui.TextWrapped("Use the config and add a doodle to get started.");
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
                ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(600, 300));
                ImGui.SetNextWindowSize(new Vector2(600, 300), ImGuiCond.FirstUseEver);
                ImGui.Begin("Pixel Perfect Jump Config", ref _config);
               
                ImGui.BeginTabBar("Config Tabs");

                if(ImGui.BeginTabItem("Config##Doodles"))
                {
                    var number2 = 0;
                    ImGui.Checkbox("Hide Updates", ref _bitch);
                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Never show any messages."); }
                    ImGui.Separator();
                    foreach (var doodle in doodleBag)
                    {
                        var enabled = doodle.Enabled;
                        var combat = doodle.Combat;
                        var instance = doodle.Instance;

                        var name = doodle.Name;
                        ImGui.Checkbox($"Enable ##{number2}", ref enabled);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Turn the doodle on/off entirely"); }
                        ImGui.SameLine();
                        ImGui.Checkbox($"Combat ##{number2}", ref combat);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Only show when engaged in combat"); }
                        ImGui.SameLine();
                        ImGui.Checkbox($"Instance ##{number2}", ref instance);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Only show when in an instance (a dungeon/raid etc)"); }
                        ImGui.SameLine();
                        ImGui.PushItemWidth(150);
                        ImGui.InputText($"Name##{number2}", ref name, 20);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Name the doodle!"); }
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
                        if (number2 + 1 < doodleBag.Count)
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
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Delete the doodle"); }
                        number2++;
                        doodle.Enabled = enabled;
                        doodle.Instance = instance;
                        doodle.Combat= combat;
                        doodle.Name = name;
                    }

                    ImGui.Separator();
                    if (ImGui.Button("Add Doodle"))
                    {
                        doodleBag.Add(new Drawing());
                    }
                    if (ImGui.Button("Show Editor"))
                    {
                        _editor = !_editor;
                    }


                    ImGui.EndTabItem();
                }

                var number = 0;
                selected = -1;
                foreach (var doodle in doodleBag)
                {

                    if (ImGui.BeginTabItem($"{doodle.Name}##{number}"))
                    {
                        selected = number;

                        var type = doodle.Type;
                        var colour = doodle.Colour;
                        var north = doodle.North;
                        var pause = doodle.Pause;       
                        var thickness = doodle.Thickness;
                        var segments = doodle.Segments;
                        var vector = doodle.Vector;
                        var filled = doodle.Filled;
                        var x1 = doodle.Vector.X;
                        var z1= doodle.Vector.Y;
                        var x2 = doodle.Vector.Z;
                        var z2 = doodle.Vector.W;
                        var radius = doodle.Radius;
                        var job = doodle.Job;
                        var jobsBool = doodle.JobsBool;
                        var offset = doodle.Offset;
                        var outline = doodle.Outline;
                        var outlineColour = doodle.OutlineColour;

                        ImGui.PushItemWidth(300);
                        ImGui.Combo($"Type ##{number}",ref type, doodleOptions, doodleOptions.Length);
                        ImGui.ColorEdit4($"Colour ##{number}", ref colour, ImGuiColorEditFlags.NoInputs);
                        if (ImGui.TreeNode($"Jobs##{number}"))
                        {
                            var loop = 0;
                            ImGui.Columns(6);
                            foreach(var jobb in doodle.JobsBool)
                            {
                                ImGui.Checkbox($"{doodleJobs[loop]}", ref jobsBool[loop]);

                                if (loop ==0 | loop==4 | loop==8 | loop==13 | loop==16)
                                {
                                    ImGui.NextColumn();
                                }
                                loop++;
                            }
                            ImGui.Columns(1);
                            ImGui.TreePop();
                        }

                        ImGui.InputFloat($"Thickness ##{number}", ref thickness, 0.1f, 1f);
                        
                        if (type == 0)//ring
                        {
                            ImGui.InputFloat($"Radius##{number}", ref radius,0.1f, 1f);
                            ImGui.InputInt($"Segments ##{number}", ref segments, 1, 10);
                            ImGui.Checkbox($"Offset##{number}", ref offset);
                            if (offset)
                            {
                                ImGui.InputFloat($"Offset X##{number}", ref x1, 0.1f, 1f);
                                ImGui.InputFloat($"Offset Y##{number}", ref z1, 0.1f, 1f);
                            }
                        }

                        if (type==1) //line
                        {
                            ImGui.Checkbox($"Locked North ##{number}", ref north);
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Otherwise, player relative"); }
                            ImGui.PushItemWidth(100);
                            ImGui.InputFloat($"X 1##{number}", ref x1, 0.1f, 1f);
                            ImGui.SameLine();
                            ImGui.InputFloat($"Y 1##{number}", ref z1, 0.1f, 1f);
                            ImGui.InputFloat($"X 2##{number}", ref x2, 0.1f, 1f);
                            ImGui.SameLine();
                            ImGui.InputFloat($"Y 2##{number}", ref z2, 0.1f, 1f);
                            ImGui.PopItemWidth();
                        }

                        if (type==2)//dot
                        {
                            ImGui.InputFloat($"Radius##{number}", ref radius,0.1f,1f);
                            ImGui.InputInt($"Segments ##{number}", ref segments, 1, 10);
                            ImGui.Checkbox($"Filled##{number}", ref filled);
                            ImGui.SameLine();
                            ImGui.Checkbox($"Offset##{number}", ref offset);
                            ImGui.SameLine();
                            ImGui.Checkbox($"Outline##{number}", ref outline);
                            if (outline)
                            {
                                ImGui.ColorEdit4($"Outline Colour ##{number}", ref outlineColour, ImGuiColorEditFlags.NoInputs);
                            }
                            if (offset)
                            {
                                ImGui.InputFloat($"Offset X##{number}", ref x1, 0.1f, 1f);
                                ImGui.InputFloat($"Offset Y##{number}", ref z1, 0.1f, 1f);
                            }
                        }
                        if (type == 3)//tracker
                        {
                            ImGui.Checkbox($"Pause tracking", ref pause);
                            if (ImGui.Button($"Clear tracker"))
                            {
                                doodle.TrackingDots.Clear();
                            }
                            ImGui.SameLine();
                            if (ImGui.Button($"Copy tracking CSV"))
                            {
                                var csv = new StringBuilder();
                                foreach (Vector3 p in doodle.TrackingDots)
                                {
                                    csv.AppendLine(string.Format("{0},{1},{2}", p.X, p.X, p.Z));
                                }
                                ImGui.SetClipboardText(csv.ToString());
                                ToastGui.ShowNormal("Copied!");
                            }
                        }
                        ImGui.PopItemWidth();
                        doodle.Type = type;
                        doodle.Colour = colour;
                        doodle.North = north;
                        doodle.Pause = pause;
                        if (thickness < 0f) { thickness = 0f; }
                        doodle.Thickness = thickness;
                        if (segments > 1000) { segments = 1000; }
                        if (segments < 4) { segments = 4; }
                        doodle.Segments = segments;
                        doodle.Vector = vector;
                        doodle.Filled = filled;
                        doodle.Radius = radius;
                        doodle.Vector = new Vector4(x1, z1, x2, z2);
                        doodle.Job = job;
                        doodle.JobsBool = jobsBool;
                        doodle.Offset = offset;
                        doodle.Outline= outline;
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

                if (dirtyHack > 100)
                {
                    SaveConfig();
                    dirtyHack = 0;
                }

                dirtyHack++;
                if (deleteNum != -1)
                {
                    doodleBag.RemoveAt(deleteNum);
                    deleteNum = -1;
                }
                if(moveNum!= -1)
                {
                    
                    var dood = doodleBag[moveNum];
                    doodleBag.RemoveAt(moveNum);
                    if (moveUp)
                    {
                        doodleBag.Insert(moveNum-1, dood);
                    }
                    else
                    {
                        doodleBag.Insert(moveNum+1, dood);
                    }
                    moveNum= -1;
                    moveUp = false;
                }
            }
        }
        }
}
