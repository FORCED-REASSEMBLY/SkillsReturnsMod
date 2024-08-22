using System;
using BepInEx;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.IO;
using System.Reflection;
using UnityEngine.Networking;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using SkillsReturns.SkillStates.Commando;

namespace SkillsReturns
{
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(R2API.DamageAPI.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]

    [BepInPlugin(
        "com.Forced_Reassembly.SkillsReturns",
        "Skills Returns",
        "1.1.0")]
    public class SkillsReturnsPlugin : BaseUnityPlugin
    {
        private void Awake()
         {
            IL.RoR2.HealthComponent.TakeDamage += (il) =>
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(
                     x => x.MatchLdarg(1),
                     x => x.MatchLdfld<DamageInfo>("damage"),
                     x => x.MatchStloc(6)
                    ))
                {
                    c.Index += 3;
                    c.Emit(OpCodes.Ldloc, 6);
                    c.Emit(OpCodes.Ldarg_0);    //self
                    c.Emit(OpCodes.Ldarg_1);    //damageInfo
                    c.EmitDelegate<Func<float, HealthComponent, DamageInfo, float>>((origDamage, victimHealth, damageInfo) =>
                    {
                        float newDamage = origDamage;
                        CharacterBody victimBody = victimHealth.body;
                        if (victimBody && victimBody.HasBuff(CommandoKnifeDebuff))
                        {
                            newDamage *= 1.5f;
                        }
                        return newDamage;
                    });
                    c.Emit(OpCodes.Stloc, 6);
                }
                else
                {
                    UnityEngine.Debug.LogError("SkillsReturns: CommandoKnifeDebuff IL Hook failed. This will break a lot of things.");
                }
            };



            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageInfo) => {
                //Always use a NetworkServer.active check when doing stuff with TakeDamage
                if (NetworkServer.active && self.body && self.body.HasBuff(CommandoKnifeDebuff) && damageInfo.damageColorIndex == DamageColorIndex.Default)
                {
                    damageInfo.damageColorIndex = DamageColorIndex.SuperBleed;    //pick whatever color you want
                }

                //ALWAYS remember to call orig so that the original function can run.
                orig(self, damageInfo);

            };

            GlobalEventManager.onServerDamageDealt += ApplyKnifeDebuff;

            using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SkillsReturns.skillsreturnsbundle"))
            {
                mainAssetBundle = AssetBundle.LoadFromStream(manifestResourceStream);
            }

            // First we must load our survivor's Body prefab. For this tutorial, we are making a skill for Commando
            // If you would like to load a different survivor, you can find the key for their Body prefab at the following link
            // https://xiaoxiao921.github.io/GithubActionCacheTest/assetPathsDump.html
            GameObject commandoBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion();

            // We use LanguageAPI to add strings to the game, in the form of tokens
            // Please note that it is instead recommended that you use a language file.
            // More info in https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
            LanguageAPI.Add("COMMANDO_SECONDARY_SLASHKNIFE_NAME", "Combat Knife");
            LanguageAPI.Add("COMMANDO_SECONDARY_SLASHKNIFE_DESCRIPTION", $"Slash enemies for <style=cIsDamage>360% damage</style>, wounding and <style=cIsDamage>stunning</style> enemies. Wounded enemies take <style=cIsDamage>50% more damage</style>.");

            // Now we must create a SkillDef
            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();

            //Check step 2 for the code of the CustomSkillsTutorial.MyEntityStates.SimpleBulletAttack class
            mySkillDef.activationState = new SerializableEntityStateType(typeof(SlashKnife));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 3f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.cancelSprintingOnActivation = true;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            // For the skill icon, you will have to load a Sprite from your own AssetBundle
            mySkillDef.icon = mainAssetBundle.LoadAsset<Sprite>("CombatKnifeIcon");
            mySkillDef.skillDescriptionToken = "COMMANDO_SECONDARY_SLASHKNIFE_DESCRIPTION";
            mySkillDef.skillName = "COMMANDO_SECONDARY_SLASHKNIFE_NAME";
            mySkillDef.skillNameToken = "COMMANDO_SECONDARY_SLASHKNIFE_NAME";
            (mySkillDef as ScriptableObject).name = mySkillDef.skillName;

            // This adds our skilldef. If you don't do this, the skill will not work.
            ContentAddition.AddSkillDef(mySkillDef);
            ContentAddition.AddEntityState(typeof(SlashKnife), out bool wasAdded);
            // Now we add our skill to one of the survivor's skill families
            // You can change component.primary to component.secondary, component.utility and component.special
            SkillLocator skillLocator = commandoBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamily = skillLocator.secondary.skillFamily;

            // If this is an alternate skill, use this code.
            // Here, we add our skill as a variant to the existing Skill Family.
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }
        public static AssetBundle mainAssetBundle; 

        // this is where your debuff/buff will be created
        public static BuffDef CreateBuffDef(string name, bool canStack, bool isCooldown, bool isDebuff, Color color, Sprite iconSprite)
        {
            BuffDef bd = ScriptableObject.CreateInstance<BuffDef>();
            bd.name = name;
            bd.canStack = canStack;
            bd.isCooldown = isCooldown; //Allows it to be cleansed by  blast shower
            bd.isDebuff = isDebuff;
            bd.buffColor = color;
            bd.iconSprite = iconSprite;
            ContentAddition.AddBuffDef(bd);

            (bd as UnityEngine.Object).name = bd.name;
            return bd;
        }

        public static BuffDef CommandoKnifeDebuff = CreateBuffDef("CommandoKnifeDebuff", false, false, true, new Color32(81, 0, 0, 255), Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Bandit2/texBuffSuperBleedingIcon.tif").WaitForCompletion());

        public static DamageAPI.ModdedDamageType CommandoKnifeDamage = DamageAPI.ReserveDamageType();

        private void ApplyKnifeDebuff(DamageReport report)
        {
            //Stop execution early if these conditions aren't met.
            if (!report.victimBody) return;
            if (report.damageInfo.rejected) return;
            if (!report.damageInfo.HasModdedDamageType(CommandoKnifeDamage)) return;

            report.victimBody.AddTimedBuff(CommandoKnifeDebuff, 5f);
        }
        
    }

}