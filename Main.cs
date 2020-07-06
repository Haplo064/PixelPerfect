using System;
using Dalamud.Plugin;
using ImGuiNET;
using Dalamud.Configuration;
using Num = System.Numerics;
using Dalamud.Game.Command;

namespace PixelPerfect
{
    public class PixelPerfect : IDalamudPlugin
    {
        public string Name => "Pixel Perfect";
        private DalamudPluginInterface pluginInterface;
        public Config Configuration;

        public bool enabled = true;
        public bool config = false;
        public bool debug = false;
        public bool combat = true;
        public bool circle = false;
        public bool instance = false;
        public Num.Vector4 col = new Num.Vector4(1f, 1f, 1f, 1f);
        public Num.Vector4 col2 = new Num.Vector4(0.4f, 0.4f, 0.4f, 1f);

        public bool ring = false;
        public Num.Vector4 col_ring = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        public float radius = 10f;
        public int segments = 100;
        public float thickness = 10f;


        public void Initialize(DalamudPluginInterface pI)
        {
            this.pluginInterface = pI;
            Configuration = pluginInterface.GetPluginConfig() as Config ?? new Config();

            ring = Configuration.Ring;
            thickness = Configuration.Thickness;
            col_ring = Configuration.Col_Ring;
            segments = Configuration.Segments;
            radius = Configuration.Radius;

            enabled = Configuration.Enabled;
            combat = Configuration.Combat;
            circle = Configuration.Circle;
            instance = Configuration.Instance;
            col = Configuration.Col;
            col2 = Configuration.Col2;


            this.pluginInterface.UiBuilder.OnBuildUi += DrawWindow;
            this.pluginInterface.UiBuilder.OnOpenConfigUi += ConfigWindow;
            this.pluginInterface.CommandManager.AddHandler("/pp", new CommandInfo(Command));
        }

        public void Dispose()
        {
            this.pluginInterface.UiBuilder.OnBuildUi -= DrawWindow;
            this.pluginInterface.UiBuilder.OnOpenConfigUi -= ConfigWindow;
            this.pluginInterface.CommandManager.RemoveHandler("/pp");
        }

        private void DrawWindow()
        {

            if (config)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(300, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Pixel Perfect Config", ref config);
                ImGui.Checkbox("Hitbox", ref enabled);
                ImGui.Checkbox("Outer Ring", ref circle);
                ImGui.Checkbox("Combat Only", ref combat);
                ImGui.Checkbox("Instance Only", ref instance);
                ImGui.ColorEdit4("Colour", ref col, ImGuiColorEditFlags.NoInputs);
                ImGui.ColorEdit4("Outer Colour", ref col2, ImGuiColorEditFlags.NoInputs);

                ImGui.Separator();
                
                ImGui.Checkbox("Ring", ref ring);
                ImGui.DragFloat("Yalms", ref radius);
                ImGui.DragFloat("Thickness", ref thickness);
                ImGui.DragInt("Smoothness", ref segments);
                ImGui.ColorEdit4("Ring Colour", ref col_ring, ImGuiColorEditFlags.NoInputs);

                if (ImGui.Button("Save and Close Config"))
                {
                    SaveConfig();
                    config = false;
                }
                ImGui.End();
            }


            if (enabled && pluginInterface.ClientState.LocalPlayer != null)
            {
                if(combat)
                {
                    if (!pluginInterface.ClientState.Condition[Dalamud.Game.ClientState.ConditionFlag.InCombat]) { return; }
                }

                if (instance)
                {
                    if (!pluginInterface.ClientState.Condition[Dalamud.Game.ClientState.ConditionFlag.BoundByDuty]) { return; }
                }

                var actor = pluginInterface.ClientState.LocalPlayer;
                if (pluginInterface.Framework.Gui.WorldToScreen(new SharpDX.Vector3(actor.Position.X, actor.Position.Z, actor.Position.Y), out SharpDX.Vector2 pos))
                {
                    ImGui.Begin("Pixel Perfect", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs);
                    ImGui.SetWindowPos(new Num.Vector2(pos.X - 10, pos.Y - 10));
                    ImGui.GetWindowDrawList().AddCircleFilled(
                        new Num.Vector2(pos.X, pos.Y),
                        2f,
                        ImGui.GetColorU32(col),
                        100);
                    if(circle)
                    {
                        ImGui.GetWindowDrawList().AddCircle(
                            new Num.Vector2(pos.X, pos.Y),
                            2.2f,
                            ImGui.GetColorU32(col2),
                            100);
                    }

                    ImGui.End();

                    if (ring)
                    {
                        ImGui.Begin("Ring", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
                        ImGui.SetWindowPos(new Num.Vector2(0, 0));
                        ImGui.SetWindowSize(new Num.Vector2(1920, 1080));
                        DrawRingWorld(pluginInterface.ClientState.LocalPlayer, radius, segments, thickness, ImGui.GetColorU32(col_ring));
                        ImGui.End();
                    }
                }
            }
        }
        private void ConfigWindow(object Sender, EventArgs args)
        {
            config = true;
        }

        public void Command(string command, string arguments)
        {
            config = true;
        }
        public void SaveConfig()
        {
            Configuration.Enabled = enabled;
            Configuration.Combat = combat;
            Configuration.Circle = circle;
            Configuration.Instance = instance;
            Configuration.Col = col;
            Configuration.Col2 = col2;
            Configuration.Col_Ring = col_ring;
            Configuration.Thickness = thickness;
            Configuration.Segments = segments;
            Configuration.Ring = ring;
            Configuration.Radius = radius;
            this.pluginInterface.SavePluginConfig(Configuration);
        }

        public void DrawRingWorld(Dalamud.Game.ClientState.Actors.Types.Actor actor, float radius, int num_segments, float thicc, uint colour)
        {
            int seg = num_segments / 2;

            for (int i = 0; i <= num_segments; i++)
            {
                pluginInterface.Framework.Gui.WorldToScreen(new SharpDX.Vector3(actor.Position.X + (radius * (float)Math.Sin((Math.PI / seg) * i)), actor.Position.Z, actor.Position.Y + (radius * (float)Math.Cos((Math.PI / seg) * i))), out SharpDX.Vector2 pos);
                ImGui.GetWindowDrawList().PathLineTo(new Num.Vector2(pos.X, pos.Y));
            }
            ImGui.GetWindowDrawList().PathStroke(colour, true, thicc);
        }

    }

    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = true;
        public bool Combat { get; set; } = true;
        public bool Circle { get; set; } = false;
        public bool Instance { get; set; } = false;
        public Num.Vector4 Col { get; set; } = new Num.Vector4(1f, 1f, 1f, 1f);
        public Num.Vector4 Col2 { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 1f);
        public Num.Vector4 Col_Ring { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        public int Segments { get; set; } = 100;
        public float Thickness { get; set; } = 10f;
        public bool Ring { get; set; } = false;
        public float Radius { get; set; } = 2f;


    }

}
