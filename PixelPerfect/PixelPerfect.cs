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
        private bool _cutscene;
        private Num.Vector4 _col = new Num.Vector4(1f, 1f, 1f, 1f);
        private Num.Vector4 _col2 = new Num.Vector4(0.4f, 0.4f, 0.4f, 1f);
        //ring
        private bool _ring;
        private Num.Vector4 _colRing = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        private float _radius = 10f;
        private int _segments = 100;
        private float _thickness = 10f;
        //ring2
        private bool _ring2;
        private Num.Vector4 _colRing2 = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        private float _radius2 = 10f;
        private int _segments2 = 100;
        private float _thickness2 = 10f;
        //north stuff
        private bool _north1;
        private bool _north2;
        private bool _north3;
        private float _lineOffset = 0.6f;
        private float _lineLength = 1f;
        private float _chevLength = 0.5f;
        private float _chevOffset = 0.5f;
        private float _chevRad = 0.5f;
        private float _chevSin = 0.5f;
        private float _chevThicc = 10f;
        private float _lineThicc = 10f;
        private Num.Vector4 _chevCol = new Num.Vector4(1f, 1f, 1f, 1f);
        private Num.Vector4 _lineCol = new Num.Vector4(1f, 1f, 1f, 1f);
        private int dirtyHack = 0;
        

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
            _ring = _configuration.Ring;
            _thickness = _configuration.Thickness;
            _colRing = _configuration.ColRing;
            _segments = _configuration.Segments;
            _radius = _configuration.Radius;
            _enabled = _configuration.Enabled;
            _combat = _configuration.Combat;
            _circle = _configuration.Circle;
            _instance = _configuration.Instance;
            _cutscene = _configuration.Cutscene;
            _col = _configuration.Col;
            _col2 = _configuration.Col2;
            _ring2 = _configuration.Ring2;
            _thickness2 = _configuration.Thickness2;
            _colRing2 = _configuration.ColRing2;
            _segments2 = _configuration.Segments2;
            _radius2 = _configuration.Radius2;
            _north1 = _configuration.North1;
            _north2 = _configuration.North2;
            _north3 = _configuration.North3;
            _lineOffset = _configuration.LineOffset;
            _lineLength = _configuration.LineLength;
            _chevLength = _configuration.ChevLength;
            _chevOffset = _configuration.ChevOffset;
            _chevRad = _configuration.ChevRad;
            _chevSin = _configuration.ChevSin;
            _chevThicc = _configuration.ChevThicc;
            _lineThicc = _configuration.LineThicc;
            _chevCol = _configuration.ChevCol;
            _lineCol = _configuration.LineCol;
            
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
                
                ImGui.Checkbox("Combat Only", ref _combat);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Only show all of this during combat");
                }
                ImGui.SameLine();
                ImGui.Checkbox("Instance Only", ref _instance);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Only show all of this during instances (like dungeons, raids etc)");
                }
                ImGui.SameLine();
                ImGui.Checkbox("Show in cutscene", ref _cutscene);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Show all of this during cutscene");
                }

                ImGui.Separator();
                ImGui.Checkbox("Hitbox", ref _enabled);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("A visual representation of your hitbox");
                }
                if (_enabled)
                {
                    ImGui.SameLine();
                    ImGui.ColorEdit4("Hitbox Colour", ref _col, ImGuiColorEditFlags.NoInputs);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("The colour of the hitbox");
                    }
                }
                ImGui.Checkbox("Outer Ring", ref _circle);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("A tiny circle of colour around the hitbox dot");
                }
                if (_circle)
                {
                    ImGui.SameLine();
                    ImGui.ColorEdit4("Outer Colour", ref _col2, ImGuiColorEditFlags.NoInputs);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("The colour of the ring");
                    }
                }
                ImGui.Checkbox("Show North", ref _north1);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("An indicator that always points north");
                }
                if (_north1)
                {
                    ImGui.SameLine();
                    ImGui.Checkbox("Line", ref _north2);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("What can I say. It's a line");
                    }
                    ImGui.SameLine();
                    ImGui.Checkbox("Chevron", ref _north3);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Fancy word for a pointer");
                    }
                    if (_north2)
                    {
                        ImGui.DragFloat("Line Offset", ref _lineOffset);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("How far from your hitbox to start the line");
                        }
                        ImGui.DragFloat("Line Length", ref _lineLength);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("How long the line is");
                        }
                        ImGui.DragFloat("Line Thickness", ref _lineThicc);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("How thicc the line is");
                        }
                        ImGui.ColorEdit4("Line Colour", ref _lineCol, ImGuiColorEditFlags.NoInputs);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("The colour of the line");
                        }
                    }
                    if (_north3)
                    {
                        ImGui.DragFloat("Chevron Offset", ref _chevOffset);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("These are all a bit iffy, due to maths. Mess around with them");
                        }
                        ImGui.DragFloat("Chevron Length", ref _chevLength);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("These are all a bit iffy, due to maths. Mess around with them");
                        }
                        ImGui.DragFloat("Chevron Radius", ref _chevRad);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("These are all a bit iffy, due to maths. Mess around with them");
                        }
                        ImGui.DragFloat("Chevron Sin", ref _chevSin);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("These are all a bit iffy, due to maths. Mess around with them");
                        }
                        ImGui.DragFloat("Chevron Thickness", ref _chevThicc);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("How thicc the Chevron is");
                        }
                        ImGui.ColorEdit4("Chevron Colour", ref _chevCol, ImGuiColorEditFlags.NoInputs);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("The colour of the Chevron");
                        }
                    }
                }

                
                
                
                ImGui.Separator();
                ImGui.Checkbox("Ring", ref _ring);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Show a ring around your character");
                }
                if (_ring)
                {
                    ImGui.DragFloat("Yalms", ref _radius);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("The radius of the ring");
                    }
                    ImGui.DragFloat("Thickness", ref _thickness);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("The thiccness of the ring");
                    }
                    ImGui.DragInt("Smoothness", ref _segments);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("How many segments to make the ring out of");
                    }
                    ImGui.ColorEdit4("Ring Colour", ref _colRing, ImGuiColorEditFlags.NoInputs);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("The colour of the ring");
                    }
                }
                ImGui.Separator();
                ImGui.Checkbox("Ring 2", ref _ring2);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Show another ring around your character");
                }
                if (_ring2)
                {
                    ImGui.DragFloat("Yalms 2", ref _radius2);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("The radius of the ring");
                    }
                    ImGui.DragFloat("Thickness 2", ref _thickness2);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("The thiccness of the ring");
                    }
                    ImGui.DragInt("Smoothness 2", ref _segments2);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("How many segments to make the ring out of");
                    }
                    ImGui.ColorEdit4("Ring Colour 2", ref _colRing2, ImGuiColorEditFlags.NoInputs);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("The colour of the ring");
                    }
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
            
            if (_combat)
            {
                if (!_condition[ConditionFlag.InCombat])
                {
                    return;
                }
            }

            if (_instance)
            {
                if (!_condition[ConditionFlag.BoundByDuty])
                {
                    return;
                }

            }

            if (!_cutscene)
            {
                var cutsceneActive = _condition[ConditionFlag.OccupiedInCutSceneEvent] ||
                                     _condition[ConditionFlag.WatchingCutscene] ||
                                     _condition[ConditionFlag.WatchingCutscene78];
                if (cutsceneActive)
                {
                    return;
                }
            }

            var actor = _cs.LocalPlayer;
            if (!_gui.WorldToScreen(
                new Num.Vector3(actor.Position.X, actor.Position.Y, actor.Position.Z),
                out var pos)) return;
            
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Num.Vector2(0, 0));
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Num.Vector2(0, 0));
            ImGui.Begin("Ring",
                ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);
            
           if(_enabled)
           {
               ImGui.GetWindowDrawList().AddCircleFilled(
                   new Num.Vector2(pos.X, pos.Y),
                   2f,
                   ImGui.GetColorU32(_col),
                   100);
           }
           if (_circle)
            {
                ImGui.GetWindowDrawList().AddCircle(
                    new Num.Vector2(pos.X, pos.Y),
                    2.2f,
                    ImGui.GetColorU32(_col2),
                    100);
            }

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
            
            ImGui.End();
            ImGui.PopStyleVar();
        }
        

        private void Command(string command, string arguments)
        {
            if (arguments == "ring")
            {
                _ring = !_ring;
            }
            else if(arguments == "ring2")
            {
                _ring2 = !_ring2;
            }
            else if(arguments == "north")
            {
                _north1 = !_north1;
            }
            else
            {
                _config = !_config;
            }
            SaveConfig();
        }

        private void SaveConfig()
        {
            _configuration.Enabled = _enabled;
            _configuration.Combat = _combat;
            _configuration.Circle = _circle;
            _configuration.Instance = _instance;
            _configuration.Col = _col;
            _configuration.Col2 = _col2;
            _configuration.ColRing = _colRing;
            _configuration.Thickness = _thickness;
            _configuration.Segments = _segments;
            _configuration.Ring = _ring;
            _configuration.Radius = _radius;
            _configuration.Ring2 = _ring2;
            _configuration.Thickness2 = _thickness2;
            _configuration.ColRing2 = _colRing2;
            _configuration.Segments2 = _segments2;
            _configuration.Radius2 = _radius2;
            _configuration.North1 = _north1;
            _configuration.North2 = _north2;
            _configuration.North3 = _north3;
            _configuration.LineOffset = _lineOffset;
            _configuration.LineLength = _lineLength;
            _configuration.ChevLength = _chevLength;
            _configuration.ChevOffset = _chevOffset;
            _configuration.ChevRad = _chevRad;
            _configuration.ChevSin = _chevSin;
            _configuration.ChevThicc = _chevThicc;
            _configuration.LineThicc = _lineThicc;
            _configuration.ChevCol = _chevCol;
            _configuration.LineCol = _lineCol;
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


    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = true;
        public bool Combat { get; set; } = true;
        public bool Circle { get; set; }
        public bool Instance { get; set; }
        public bool Cutscene { get; set; }
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