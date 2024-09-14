using BepInEx;
using R2API;
using SkillsReturns.SkillSetup;
using System.Linq;
using System.Reflection;

namespace SkillsReturns
{
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(DamageAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(SoundAPI.PluginGUID)]
    [BepInDependency(R2API.R2API.PluginGUID)]

    [BepInPlugin(
        "com.Forced_Reassembly.SkillsReturns",
        "Skills Returns",
        "1.1.0")]
    public class SkillsReturnsPlugin : BaseUnityPlugin
    {
        public static BepInEx.Configuration.ConfigFile configFile;

        private void Awake()
        {
            configFile = base.Config;
            Assets.Init();

            AddToAssembly();
        }

        //Based off of TILER2 Item Code
        //Scans for all SkillBase in the project and automatically sets them up.
        private void AddToAssembly()
        {
            var SkillTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(SkillBase)));
            foreach (var skillType in SkillTypes)
            {
                var instance = (SkillBase) System.Activator.CreateInstance(skillType);
            }
        }
    }
}