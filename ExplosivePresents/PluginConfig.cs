using BepInEx.Configuration;

namespace Hypick;

public static class Categories
{
	public const string Present = nameof(Present);
}

public class PluginConfig
{
	public bool ImmediateExplosion;
	public float SpawnChance;
	public float KillRange;
	public float DamageRange;
	public float Delay;

	public PluginConfig(ConfigFile cfg)
	{
		ImmediateExplosion = cfg.Bind<bool>(Categories.Present, nameof(ImmediateExplosion), false, "").Value;
		SpawnChance = cfg.Bind<float>(Categories.Present, nameof(SpawnChance), 10f, new ConfigDescription("Chance of the present exploding", new AcceptableValueRange<float>(0f, 100f))).Value;
		KillRange = cfg.Bind<float>(Categories.Present, nameof(KillRange), 5.7f, "Explosion kill range").Value;
		DamageRange = cfg.Bind<float>(Categories.Present, nameof(DamageRange), 6.4f, "Explosion damage range").Value;
		Delay = cfg.Bind<float>(Categories.Present, nameof(Delay), 0.5f, "Delay before explosion. In seconds").Value;
	}
}
