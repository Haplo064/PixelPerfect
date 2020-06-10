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
        public Num.Vector4 col = new Num.Vector4(1f, 1f, 1f, 1f);

        public void Initialize(DalamudPluginInterface pI)
        {
            this.pluginInterface = pI;
            Configuration = pluginInterface.GetPluginConfig() as Config ?? new Config();


            try
            { enabled = Configuration.Enabled; }
            catch (Exception)
            {
                PluginLog.LogError("Failed to set Enabled");
                enabled = false;
            }

            try
            { combat = Configuration.Combat; }
            catch (Exception)
            {
                PluginLog.LogError("Failed to set Combat");
                combat = true;
            }

            try
            {
                if (Configuration.Col != null)
                {
                    col = Configuration.Col;
                }
                else
                {
                    col = new Num.Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                }
            }
            catch (Exception)
            {
                PluginLog.LogError("Failed to set col");
                col = new Num.Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            }

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
                ImGui.Checkbox("Enable", ref enabled);
                ImGui.Checkbox("Combat Only", ref combat);
                ImGui.ColorEdit4("Colour", ref col, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel);
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

                var actor = pluginInterface.ClientState.LocalPlayer;
                if (pluginInterface.Framework.Gui.WorldToScreen(new SharpDX.Vector3(actor.Position.X, actor.Position.Z, actor.Position.Y), out SharpDX.Vector2 pos))
                {
                    ImGui.Begin("Pixel Perfect", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground);
                    ImGui.SetWindowPos(new Num.Vector2(pos.X - 10, pos.Y - 10));
                    ImGui.GetWindowDrawList().AddCircleFilled(
                        new Num.Vector2(pos.X, pos.Y),
                        2f,
                        ImGui.GetColorU32(col),
                        100);

                    ImGui.End();
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
            Configuration.Col = col;
            this.pluginInterface.SavePluginConfig(Configuration);
        }
    }

    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = true;
        public bool Combat { get; set; } = true;
        public Num.Vector4 Col { get; set; } = new Num.Vector4(1f, 1f, 1f, 1f);

    }

}
