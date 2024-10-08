using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
namespace SkillsReturns
{
    internal static class ModCompat
    {
        internal static void Init()
        {
            AncientScepterCompat.Init();
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
