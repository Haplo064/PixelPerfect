using System;
using Dalamud.Configuration;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using ImGuiNET;
using Num = System.Numerics;
using System.Collections.Generic;
using System.Numerics;
using Condition = Dalamud.Game.ClientState.Conditions.Condition;
using Dalamud.Game.ClientState.Objects.Types;


namespace PixelPerfect
{
    public partial class PixelPerfect : IDalamudPlugin
    {
        public string Name => "Pixel Perfect";
        private readonly DalamudPluginInterface _pi;
        private readonly CommandManager _cm;
        private readonly ClientState _cs;
        private readonly Framework _fw;
        private readonly GameGui _gui;
        private readonly Condition _condition;
        private readonly Config _configuration;
        private bool _config;
        private bool _editor;
        private bool _firstTime;
        private bool _editorHelp;
        private int _dirtyHack;
        private readonly string[] _doodleOptions;
        private readonly string[] _doodleJobs;
        private readonly uint[] _doodleJobsUint;
        private readonly List<Drawing> _doodleBag;
        private float _editorScale;
        private int _selected;
        private int _grabbed = -1;
        private bool _update;
        private bool _bitch;
        private const int Version = 4;
        
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
            
            _bitch= _configuration.Bitch;

            _doodleBag = _configuration.DoodleBag;
            if(_doodleBag.Count==0 ) { _firstTime = true; }

            //update popup
            if(_configuration.Version < Version)
            {
                if (!_bitch) { _update = true; }
            }

            //Update adding jobs
            if (_configuration.Version < 4)
            {
                if (!_bitch) { _update = true; }
                _configuration.Version = 4;
                foreach(var doodle in _doodleBag)
                {
                    if (doodle.Job > 0)
                    {
                        doodle.JobsBool[0] = false;
                        doodle.JobsBool[doodle.Job] = true;
                    }
                }
                SaveConfig();
            }

            _doodleOptions = new[]{ "Ring","Line","Dot"};
            _doodleJobs = new[] { "All", "PLD", "WAR", "DRK", "GNB", "WHM", "SCH", "AST", "SGE", "MNK", "DRG", "NIN", "SAM", "RPR", "BRD", "MCH", "DNC", "BLM", "SMN",  "RDM","BLU" };
            _doodleJobsUint = new uint[] { 0, 19, 21, 32, 37, 24, 28, 33, 40, 20, 22, 30, 34, 39, 23, 31, 38, 25, 27, 35, 36 };
            

            _editorScale = 4f;
            _selected = -1;

            pluginInterface.UiBuilder.Draw += DrawConfig;
            pluginInterface.UiBuilder.Draw += DrawDoodles;
            pluginInterface.UiBuilder.Draw += DrawEditor;
            pluginInterface.UiBuilder.OpenConfigUi += ConfigWindow;
            commandManager.AddHandler("/pp", new CommandInfo(Command)
            {
                HelpMessage = "Pixel Perfect config."
            }) ;
        }

        private void ConfigWindow()
        {
            _config = true;
        }


        public void Dispose()
        {
            _pi.UiBuilder.Draw -= DrawConfig;
            _pi.UiBuilder.Draw -= DrawDoodles;
            _pi.UiBuilder.Draw -= DrawEditor;
            _pi.UiBuilder.OpenConfigUi -= ConfigWindow;
            _cm.RemoveHandler("/pp");
        }

        private void Command(string command, string arguments)
        {
             _config = !_config;
            SaveConfig();
        }

        private bool CheckJob(uint jobUint, bool[] jobList)
        {
            var check = jobList[0];
            var loop = 0;
            foreach(var job in jobList )
            {
                if ( job )
                {
                    if (_doodleJobsUint[loop] == jobUint)
                    {
                        check = true;
                    }
                }
                loop++;
            }

            return check;
        }
        
        private void SaveConfig()
        {
            _configuration.DoodleBag = _doodleBag;
            _configuration.Bitch = _bitch;
            _pi.SavePluginConfig(_configuration);
        }

        private void DrawRingWorld(GameObject actor, float radius, int numSegments, float thicc, uint colour, bool offset, bool rotateOffset, Vector4 off)
        {
            var xOff = 0f;
            var yOff = 0f;
            if (offset)
            {
                xOff = off.X;
                yOff = off.Y;
                if (rotateOffset)
                {
                    var angle = -actor.Rotation;
                    var cosTheta = MathF.Cos(angle);
                    var sinTheta = MathF.Sin(angle);
                    xOff = cosTheta * off.X - sinTheta * off.Y;
                    yOff = sinTheta * off.X + cosTheta * off.Y;
                }
                
            }
            var seg = numSegments / 2;
            for (var i = 0; i <= numSegments; i++)
            {
                _gui.WorldToScreen(new Vector3(
                    actor.Position.X + xOff + (radius * (float)Math.Sin((Math.PI / seg) * i)),
                    actor.Position.Y,
                    actor.Position.Z + yOff + (radius * (float)Math.Cos((Math.PI / seg) * i))
                    ),
                    out Vector2 pos);
                ImGui.GetWindowDrawList().PathLineTo(new Vector2(pos.X, pos.Y));
            }
            ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.None, thicc);
        }

        private static void DrawRingEditor(float dX, float dY, float radius, int numSegments, float thicc, uint colour)
        {
            var seg = numSegments / 2;
            for (var i = 0; i <= numSegments; i++)
            {
                var dX2 = dX + (radius * (float)Math.Sin((Math.PI / seg) * i));
                var dY2 = dY + (radius * (float)Math.Cos((Math.PI / seg) * i));

                ImGui.GetWindowDrawList().PathLineTo(new Vector2(dX2, dY2));
            }
            ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.None, thicc);
        }

        public static JobIds IdToJob(uint job) => job < 19 ? JobIds.OTHER : (JobIds)job;
    }

    [Serializable]
    public class Drawing
    {
        public string Name { get; set; } = "Doodle";
        public bool Enabled { get; set; } = true;
        public int Job { get; set; }
        public bool[] JobsBool { get; set; } = { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        public int Type { get; set; } = 2;
        public Vector4 Colour { get; set; } = new(1f, 1f, 1f, 1f);
        public bool North { get; set; } = true;
        public float Thickness { get; set; } = 10f;
        public int Segments { get; set; } = 100;
        public float Radius { get; set; } = 2f;
        public Vector4 Vector { get; set; } = new( -2f, 0f, 0f, 0f);
        public bool Filled { get; set; } =  true;
        public bool Combat { get; set; }
        public bool Instance { get; set; }
        public bool Unsheathed { get; set; }
        public bool Offset { get; set; }
        public bool RotateOffset { get; set; }
        public bool Outline { get; set; }
        public Vector4 OutlineColour { get; set; } = new(1f, 1f, 1f, 1f);
    }


    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 4;
        public bool Bitch { get; set; }
        public List<Drawing> DoodleBag { get; set; } = new();
    }

    public enum JobIds : uint
    {
        OTHER = 0,
        GNB = 37,
        AST = 33,
        PLD = 19,
        WAR = 21,
        DRK = 32,
        SCH = 28,
        WHM = 24,
        BRD = 23,
        DRG = 22,
        SMN = 27,
        SAM = 34,
        BLM = 25,
        RDM = 35,
        MCH = 31,
        DNC = 38,
        NIN = 30,
        MNK = 20,
        BLU = 36,
        RPR = 39,
        SGE = 40
    }
}
