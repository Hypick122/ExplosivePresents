using BepInEx.Configuration;

namespace ExplosivePresents.Core
{
    public struct Categories
    {
        public const string Present = "Present";
    }

    public class PluginConfig
    {
        public int SpawnChance;
        public float KillRange;
        public float DamageRange;
        public float Delay;

        public PluginConfig(ConfigFile cfg)
        {
            SpawnChance = cfg.Bind<int>(Categories.Present, "spawnChance", 10, "Chance of the present exploding").Value;
            KillRange = cfg.Bind<float>(Categories.Present, "killRange", 10f, "Explosion kill range").Value;
            DamageRange = cfg.Bind<float>(Categories.Present, "damageRange", 10f, "Explosion damage range").Value;
            Delay = cfg.Bind<float>(Categories.Present, "delay", 0.5f, "Delay before explosion. In seconds").Value;
        }
    }
}
