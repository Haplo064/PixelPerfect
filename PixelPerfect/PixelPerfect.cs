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
using Num = System.Numerics;
using System.Collections.Generic;

namespace PixelPerfect
{
    public class PixelPerfect : IDalamudPlugin
    {
        public string Name => "Pixel Perfect";
        private readonly DalamudPluginInterface _pi;
        private readonly CommandManager _cm;
        private readonly ClientState _cs;
        private readonly Framework _fw;
        private readonly GameGui _gui;
        private readonly Condition _condition;
        private readonly Config _configuration;
        private bool _enabled;
        private bool _config;
        private bool _combat;
        private bool _circle;
        private bool _instance;
        private int dirtyHack = 0;
        private  List<Drawing> doodleBag;


        public PixelPerfect(
            DalamudPluginInterface pluginInterface,
            CommandManager commandManager,
            ClientState clientState,
            Framework framework,
            GameGui gameGui,
            Condition condition
        )
        {
            _pi = pluginInterface;
            _cm = commandManager;
            _cs = clientState;
            _fw = framework;
            _gui = gameGui;
            _condition = condition;

            _configuration = pluginInterface.GetPluginConfig() as Config ?? new Config();
            _enabled = _configuration.Enabled;
            _combat = _configuration.Combat;
            _circle = _configuration.Circle;
            _instance = _configuration.Instance;
            doodleBag = new List<Drawing>();

            pluginInterface.UiBuilder.Draw += DrawWindow;
            pluginInterface.UiBuilder.OpenConfigUi += ConfigWindow;
            commandManager.AddHandler("/pp", new CommandInfo(Command)
            {
                HelpMessage = "Pixel Perfect config." +
                              "\nArguments of 'ring', 'ring2', 'north' will enable/disable those features."
            });
        }

        private void ConfigWindow()
        {
            _config = true;
        }


        public void Dispose()
        {
            _pi.UiBuilder.Draw -= DrawWindow;
            _pi.UiBuilder.OpenConfigUi -= ConfigWindow;
            _cm.RemoveHandler("/pp");
        }


        private void DrawWindow()
        {
            if (_config)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(300, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Pixel Perfect Config", ref _config);

                if (ImGui.Button("Add Doodle"))
                {
                    doodleBag.Add(new Drawing());
                }

                var number = 0;
                foreach (var doodle in doodleBag)
                {
                    var enabled = doodle.Enabled;
                    var type = doodle.Type;
                    var colour = doodle.Colour;
                    var posxy = doodle.Pos;
                    var north = doodle.North;
                    var thickness = doodle.Thickness;
                    var segments = doodle.Segments;
                    var radius = doodle.Radius;
                    var offset = doodle.Offset;
                    var length = doodle.Length;
                    var vector = doodle.Vector;
                    var filled = doodle.Filled;

                    ImGui.Checkbox($"Enable##{number}", ref enabled);
                    ImGui.InputInt($"Type##{number}", ref type);
                    ImGui.ColorEdit4($"Colour##{number}", ref colour, ImGuiColorEditFlags.NoInputs);
                    //ImGui.Checkbox($"Player##{number}", ref posxy);
                    ImGui.Checkbox($"Locked North##{number}", ref north);
                    ImGui.DragFloat($"Thickness{number}",ref thickness, 1,0,99);
                    ImGui.DragInt($"Segments{number}", ref segments, 1, 0, 999);
                    ImGui.DragFloat($"Radius{number}", ref radius, 0.1f, 0, 99);
                    ImGui.DragFloat($"Offset{number}", ref offset, 0.1f, 0, 99);
                    ImGui.DragFloat($"Length{number}", ref length, 0.1f, 0, 99); //Remove?
                    //Vector (todo)
                    ImGui.Checkbox($"Filled##{number}", ref filled);

                    doodle.Enabled = enabled;
                    doodle.Type = type;
                    doodle.Colour = colour;
                    doodle.Pos = posxy;
                    doodle.North = north;
                    doodle.Thickness = thickness;
                    doodle.Segments = segments;
                    doodle.Radius = radius;
                    doodle.Offset = offset;
                    doodle.Length = length;
                    doodle.Vector = vector;
                    doodle.Filled = filled;
                    number++;

                    ImGui.Separator();
                } 

    

                if (ImGui.Button("Save and Close Config"))
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
                ImGui.End();

                if (dirtyHack > 100)
                {
                    SaveConfig();
                    dirtyHack = 0;
                }

                dirtyHack++;
            }

            if (_cs.LocalPlayer == null) return;

            var actor = _cs.LocalPlayer;
            if (!_gui.WorldToScreen(
                new Num.Vector3(actor.Position.X, actor.Position.Y, actor.Position.Z),
                out var pos)) return;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Num.Vector2(0, 0));
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Num.Vector2(0, 0));
            ImGui.Begin("Canvas",
                ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

            foreach(var doodle in doodleBag)
            {
                if (!doodle.Enabled) continue;
                if (doodle.Type == 0)
                {
                    DrawRingWorld(_cs.LocalPlayer, doodle.Radius, doodle.Segments, doodle.Thickness, ImGui.GetColorU32(doodle.Colour));
                }
                if(doodle.Type == 1)
                {

                }
                if (doodle.Type == 2)
                {
                    if (doodle.Filled)
                    {
                        ImGui.GetWindowDrawList().AddCircleFilled(new Num.Vector2(pos.X, pos.Y), doodle.Radius, ImGui.GetColorU32(doodle.Colour), doodle.Segments);
                    }
                    else 
                    {
                        ImGui.GetWindowDrawList().AddCircle(new Num.Vector2(pos.X, pos.Y), doodle.Radius, ImGui.GetColorU32(doodle.Colour), doodle.Segments, doodle.Thickness);
                    }
                    
                }

            }

            /*
            if (_ring)
            {
                DrawRingWorld(_cs.LocalPlayer, _radius, _segments, _thickness,
                    ImGui.GetColorU32(_colRing));
            }
            if (_ring2)
            {
                DrawRingWorld(_cs.LocalPlayer, _radius2, _segments2, _thickness2,
                    ImGui.GetColorU32(_colRing2));
            }

            if (_north1)
            {
                //Tip of arrow
                _gui.WorldToScreen(new Num.Vector3(
                            actor.Position.X + ((_lineLength + _lineOffset) * (float)Math.Sin(Math.PI)),
                            actor.Position.Y,
                            actor.Position.Z + ((_lineLength + _lineOffset) * (float)Math.Cos(Math.PI))
                        ),
                        out Num.Vector2 lineTip);
                //Player + offset
                _gui.WorldToScreen(new Num.Vector3(
                        actor.Position.X + (_lineOffset * (float)Math.Sin(Math.PI)),
                        actor.Position.Y,
                        actor.Position.Z + (_lineOffset * (float)Math.Cos(Math.PI))
                    ),
                    out Num.Vector2 lineOffset);
                //Chev offset1
                _gui.WorldToScreen(new Num.Vector3(
                        actor.Position.X + (_chevOffset * (float)Math.Sin(Math.PI / _chevRad) * _chevSin),
                        actor.Position.Y,
                        actor.Position.Z + (_chevOffset * (float)Math.Cos(Math.PI / _chevRad) * _chevSin)
                    ),
                    out Num.Vector2 chevOffset1);
                //Chev offset2
                _gui.WorldToScreen(new Num.Vector3(
                        actor.Position.X + (_chevOffset * (float)Math.Sin(Math.PI / -_chevRad) * _chevSin),
                        actor.Position.Y,
                        actor.Position.Z + (_chevOffset * (float)Math.Cos(Math.PI / -_chevRad) * _chevSin)
                    ),
                    out Num.Vector2 chevOffset2);
                //Chev Tip
                _gui.WorldToScreen(new Num.Vector3(
                        actor.Position.X + ((_chevOffset + _chevLength) * (float)Math.Sin(Math.PI)),
                        actor.Position.Y,
                        actor.Position.Z + ((_chevOffset + _chevLength) * (float)Math.Cos(Math.PI))
                    ),
                    out Num.Vector2 chevTip);
                if (_north2)
                {
                    ImGui.GetWindowDrawList().AddLine(new Num.Vector2(lineTip.X, lineTip.Y), new Num.Vector2(lineOffset.X, lineOffset.Y),
                        ImGui.GetColorU32(_lineCol), _lineThicc);
                }
                if (_north3)
                {
                    ImGui.GetWindowDrawList().AddLine(new Num.Vector2(chevTip.X, chevTip.Y), new Num.Vector2(chevOffset1.X, chevOffset1.Y),
                        ImGui.GetColorU32(_chevCol), _chevThicc);
                    ImGui.GetWindowDrawList().AddLine(new Num.Vector2(chevTip.X, chevTip.Y), new Num.Vector2(chevOffset2.X, chevOffset2.Y),
                        ImGui.GetColorU32(_chevCol), _chevThicc);
                }
            
            }
            */

            ImGui.End();
            ImGui.PopStyleVar();
        }


        private void Command(string command, string arguments)
        {
             _config = !_config;
            SaveConfig();
        }

        private void SaveConfig()
        {
            _configuration.Enabled = _enabled;
            _configuration.Combat = _combat;
            _configuration.Circle = _circle;
            _configuration.Instance = _instance;
            _pi.SavePluginConfig(_configuration);
        }

        private void DrawRingWorld(Dalamud.Game.ClientState.Objects.Types.Character actor, float radius, int numSegments, float thicc, uint colour)
        {
            var seg = numSegments / 2;
            for (var i = 0; i <= numSegments; i++)
            {
                _gui.WorldToScreen(new Num.Vector3(
                    actor.Position.X + (radius * (float)Math.Sin((Math.PI / seg) * i)),
                    actor.Position.Y,
                    actor.Position.Z + (radius * (float)Math.Cos((Math.PI / seg) * i))
                    ),
                    out Num.Vector2 pos);
                ImGui.GetWindowDrawList().PathLineTo(new Num.Vector2(pos.X, pos.Y));
            }
            ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.None, thicc);
        }
    }

    public class Drawing
    {
        public String Name { get; set; } = "";
        public bool Enabled { get; set; } = true;
        //add class restriction
        //add type (melee/ranged/medic etc) restrictions
        //add instance restrictions
        //add doing (casting etc) resctictions

        //0 = Ring
        //1 = Line
        //2 = Circle
        public int Type { get; set; } = 0;
        public Num.Vector4 Colour { get; set; } = new Num.Vector4(1f, 1f, 1f, 1f);
        public bool Pos { get; set; } = true;
        public bool North { get; set; } = true;
        public float Thickness { get; set; } = 10f;
        //Circles only
        public int Segments { get; set; } = 100;
        public float Radius { get; set; } = 2f;
        //Lines only
        public float Offset { get; set; } = 0.5f;
        public float Length { get; set; } = 1f;
        public Num.Vector2 Vector { get; set; } = new Num.Vector2( 0f, 0f);
        //Circles only
        public bool Filled { get; set; } =  true;
    }


    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = true;
        public bool Combat { get; set; } = true;
        public bool Circle { get; set; }
        public bool Instance { get; set; }
        public Num.Vector4 Col { get; set; } = new Num.Vector4(1f, 1f, 1f, 1f);
        public Num.Vector4 Col2 { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 1f);
        public Num.Vector4 ColRing { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        public int Segments { get; set; } = 100;
        public float Thickness { get; set; } = 10f;
        public bool Ring { get; set; }
        public float Radius { get; set; } = 2f;
        public bool Ring2 { get; set; }
        public float Radius2 { get; set; } = 2f;
        public Num.Vector4 ColRing2 { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        public int Segments2 { get; set; } = 100;
        public float Thickness2 { get; set; } = 10f;
        public bool North1 { get; set; } = false;
        public bool North2 { get; set; } = false;
        public bool North3 { get; set; } = false;
        public float LineOffset { get; set; } = 0.5f;
        public float LineLength { get; set; } = 1f;
        public float ChevLength { get; set; } = 1f;
        public float ChevOffset { get; set; } = 1f;
        public float ChevRad { get; set; } = 11.5f;
        public float ChevSin { get; set; } = -1.5f;
        public float ChevThicc { get; set; } = 5f;
        public float LineThicc { get; set; } = 5f;
        public Num.Vector4 ChevCol { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        public Num.Vector4 LineCol { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
    }
}