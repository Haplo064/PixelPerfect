using System;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using PixelPerfect.Data;
using PixelPerfect.Windows;

namespace PixelPerfect;

public class PixelPerfect : IDalamudPlugin
{
    public string Name => "Pixel Perfect";
    
    private readonly ConfigWindow _configWindow;
    private readonly Config _config;

    public PixelPerfect(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        _config = pluginInterface.GetPluginConfig() as Config ?? new Config();
        _configWindow = new ConfigWindow();
        _configWindow.Init(_config);

        pluginInterface.UiBuilder.Draw += Draw;
        pluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;
        
        Service.CommandManager.AddHandler("/pp", new CommandInfo(Command)
        {
            HelpMessage = "Pixel Perfect config." +
                          "\nArguments of 'ring', 'ring2', 'north' will enable/disable those features."
        });
    }
    
    private void Draw()
    {
        _configWindow.Draw();
    }

    private void DrawConfigUi()
    {
        _configWindow.Visible = !_configWindow.Visible;
    }

    public void Dispose()
    {
        Service.PluginInterface.UiBuilder.Draw -= Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUi;
        Service.CommandManager.RemoveHandler("/pp");
        GC.SuppressFinalize(this);
    }


    private void Command(string command, string arguments)
    {
        switch (arguments)
        {
            case "ring":
                _config.Ring = !_config.Ring;
                _config.Save();
                break;
            case "ring2":
                _config.Ring2 = !_config.Ring2;
                _config.Save();
                break;
            case "north":
                _config.North1 = !_config.North1;
                _config.Save();
                break;
            default:
                _configWindow.Visible = !_configWindow.Visible;
                break;
        }
    }
}