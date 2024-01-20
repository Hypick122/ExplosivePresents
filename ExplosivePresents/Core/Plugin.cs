using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ExplosivePresents.Core
{
    [BepInPlugin(Metadata.PLUGIN_GUID, Metadata.PLUGIN_NAME, Metadata.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance { get; private set; }

        private Harmony harmony;
        public new static PluginConfig Config { get; private set; }
        internal new static ManualLogSource Logger { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;

            Logger = base.Logger;
            Config = new PluginConfig(base.Config);

            harmony = new Harmony(Metadata.PLUGIN_GUID);
            harmony.PatchAll();

            Logger.LogInfo($"Plugin {Metadata.PLUGIN_GUID} is loaded!");
        }
    }
}
