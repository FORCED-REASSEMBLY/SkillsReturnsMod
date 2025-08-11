using RoR2.Skills;
using System.Runtime.CompilerServices;
namespace SkillsReturns
{
    internal static class ModCompat
    {
        internal static void Init()
        {
            AncientScepterCompat.Init();
            LinearDamage.pluginLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.RiskyLives.LinearDamage");
        }

        internal static class LinearDamage
        {
            public static bool pluginLoaded;
        }

        internal static class AncientScepterCompat
        {
            public static bool pluginLoaded = false;

            internal static void Init()
            {
                pluginLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter");
            }

            //Lock AncientScepter-specific code behind a mod load check.
            public static void AddScepterSkill(SkillDef scepterSkill, string targetBodyName, SkillDef originalSkill)
            {
                if (!pluginLoaded) return;
                AddScepterSkillInternal(scepterSkill, targetBodyName, originalSkill);
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            private static void AddScepterSkillInternal(SkillDef scepterSkill, string targetBodyName, SkillDef originalSkill)
            {
               // AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(scepterSkill, targetBodyName, originalSkill);
            }
        }
    }
}
