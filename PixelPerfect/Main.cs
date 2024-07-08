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
using Condition = Dalamud.Game.ClientState.Conditions.ConditionFlag;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace PixelPerfect
{
    public partial class PixelPerfect : IDalamudPlugin
    {
        public string Name => "Pixel Perfect";
        private readonly IDalamudPluginInterface _pi;
        private readonly ICommandManager _cm;
        private readonly IClientState _cs;
        private readonly IFramework _fw;
        private readonly IGameGui _gui;
        private readonly ICondition _condition;
        private readonly INotificationManager _nm;
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
        private const int Version = 7;
         
        public PixelPerfect(
            IDalamudPluginInterface pluginInterface,
            ICommandManager commandManager,
            IClientState clientState,
            IFramework framework,
            IGameGui gameGui,
            ICondition condition,
            INotificationManager notificationManager
        )
        {
            _pi = pluginInterface;
            _cm = commandManager;
            _cs = clientState;
            _fw = framework;
            _gui = gameGui;
            _condition = condition;
            _nm = notificationManager;

            _configuration = pluginInterface.GetPluginConfig() as Config ?? new Config();
            
            _bitch= _configuration.Bitch;

            _doodleBag = _configuration.DoodleBag;
            if(_doodleBag.Count==0 ) { _firstTime = true; }

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

            //Adding two new jobs to list
            if (_configuration.Version < 7)
            {
                if (!_bitch) { _update = true; }
                _configuration.Version = 7;
                foreach (var doodle in _doodleBag)
                {
                    doodle.JobsBool = AddElementToArray(doodle.JobsBool, false);
                    doodle.JobsBool = AddElementToArray(doodle.JobsBool, false);
                    bool[] JobTemp = doodle.JobsBool;
                    JobTemp[22] = doodle.JobsBool[20];//Add in PCT
                    for(int i = 14; i < 20;i++)//Add in VPR
                    {
                        JobTemp[i + 1] = doodle.JobsBool[i];
                    }
                    JobTemp[14] = false;
                    JobTemp[21] = false;
                    doodle.JobsBool = JobTemp;
                }
                     
                    SaveConfig();
            }

            //update popup
            if (_configuration.Version < Version)
            {
                _configuration.Version = Version;
                SaveConfig();
                if (!_bitch) { _update = true; }
            }

            _doodleOptions = new[]{ "Ring","Line","Dot","Dashed Ring","Cone"};
            _doodleJobs = new[] {
                "All",
                "PLD", "WAR", "DRK", "GNB",
                "WHM", "SCH", "AST", "SGE",
                "MNK", "DRG", "NIN", "SAM", "RPR", "VPR",
                "BRD", "MCH", "DNC",
                "BLM", "SMN",  "RDM", "PCT",
                "BLU" };
            _doodleJobsUint = new uint[] {
                0,
                19, 21, 32, 37,
                24, 28, 33, 40,
                20, 22, 30, 34, 39,41,
                23, 31, 38,
                25, 27, 35,42,
                36 };
            

            _editorScale = 4f;
            _selected = -1;

            pluginInterface.UiBuilder.OpenMainUi += ConfigWindow;
            pluginInterface.UiBuilder.Draw += DrawDoodles;
            pluginInterface.UiBuilder.Draw += DrawEditor;
            pluginInterface.UiBuilder.Draw += DrawConfig;
            //luginInterface.UiBuilder.OpenConfigUi += ConfigWindow;
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

        static bool[] AddElementToArray(bool[] array, bool element)
        {
            // Create a new array with increased size
            bool[] newArray = new bool[array.Length + 1];

            // Copy existing elements to the new array
            Array.Copy(array, newArray, array.Length);

            // Add the new element to the end of the new array
            newArray[newArray.Length - 1] = element;

            return newArray;
        }
        private void DrawConeWorld(IGameObject actor, float radius, int numSegments, float thicc, uint colour, bool offset, bool rotateOffset, Vector4 off, bool north, bool fill, bool target, float zed)
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
            float segAng = MathF.Tau / 360;

            _gui.WorldToScreen(new Vector3(
                actor.Position.X + xOff,actor.Position.Y+zed,actor.Position.Z + yOff ), out Vector2 pos1);
            ImGui.GetWindowDrawList().PathLineTo(new Vector2(pos1.X, pos1.Y));

            int degs = 0;
            if (!north)
            {
                degs = (int)(_cs.LocalPlayer.Rotation*(180/Math.PI));
            }

            if (_cs.LocalPlayer.TargetObject != null && target)
            {
                var atan = Math.Atan2(_cs.LocalPlayer.TargetObject.Position.X - _cs.LocalPlayer.Position.X, _cs.LocalPlayer.TargetObject.Position.Z - _cs.LocalPlayer.Position.Z);
                var degr = atan * (180 / Math.PI);
                degs = (int)degr;
            }

            for (var i = 0+degs-numSegments/2; i < numSegments/2 +degs; i++)
            {
                _gui.WorldToScreen(new Vector3(
                    actor.Position.X + xOff + (radius * MathF.Sin(segAng * i)),
                    actor.Position.Y+zed,
                    actor.Position.Z + yOff + (radius * MathF.Cos(segAng * i))
                    ),
                    out Vector2 pos);
                ImGui.GetWindowDrawList().PathLineTo(new Vector2(pos.X, pos.Y));
            }
            if (fill) { ImGui.GetWindowDrawList().PathFillConvex(colour); }
            else { ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.Closed, thicc); }
        }
        private void DrawRingWorld(IGameObject actor, float radius, int numSegments, float thicc, uint colour, bool offset, bool rotateOffset, Vector4 off, bool fill, float zed)
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
            float segAng = MathF.Tau / numSegments;
            for (var i = 0; i < numSegments; i++)
            {
                _gui.WorldToScreen(new Vector3(
                    actor.Position.X + xOff + (radius * MathF.Sin(segAng * i)),
                    actor.Position.Y+zed,
                    actor.Position.Z + yOff + (radius * MathF.Cos(segAng * i))
                    ),
                    out Vector2 pos);
                ImGui.GetWindowDrawList().PathLineTo(new Vector2(pos.X, pos.Y));
            }
            if (fill) { ImGui.GetWindowDrawList().PathFillConvex(colour); }
            else { ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.Closed, thicc); }
           
        }

        private static void DrawRingEditor(float dX, float dY, float radius, int numSegments, float thicc, uint colour)
        {
            float segAng = MathF.Tau / numSegments;
            for (var i = 0; i < numSegments; i++)
            {
                var dX2 = dX + (radius * MathF.Sin(segAng * i));
                var dY2 = dY + (radius * MathF.Cos(segAng * i));

                ImGui.GetWindowDrawList().PathLineTo(new Vector2(dX2, dY2));
            }
            ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.Closed, thicc);
        }

        public static JobIds IdToJob(uint job) => job < 19 ? JobIds.OTHER : (JobIds)job;
    }



    [Serializable]
    public class Drawing
    {
        public string Name { get; set; } = "Doodle";
        public bool Enabled { get; set; } = true;
        public int Job { get; set; }
        public bool[] JobsBool { get; set; } = { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false,false,false };
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
        public float Zed { get; set; } = 0f;
        public bool Zedding { get; set; } = false;
    }


    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 5;
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
        SGE = 40,
        VPR=41,
        PCT=42
    }
}
