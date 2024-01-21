using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Hypick.Services;

namespace Hypick;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; set; }

    public static ManualLogSource Log => Instance.Logger;

    public new static PluginConfig Config;

    private readonly Harmony _harmony = new(PluginInfo.PLUGIN_GUID);

    public TemplateService Service;

    public Plugin()
    {
        Instance = this;
    }

    private void Awake()
    {
        Config = new PluginConfig(base.Config);
        
        Service = new TemplateService();

        Log.LogInfo($"Applying patches...");
        _harmony.PatchAll();
        Log.LogInfo($"Patches applied");

        Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is fully loaded!");
    }
}
