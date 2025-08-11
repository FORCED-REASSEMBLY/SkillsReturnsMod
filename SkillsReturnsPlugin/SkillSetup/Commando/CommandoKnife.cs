using EntityStates;
using RoR2;
using R2API;
using RoR2.Skills;
using SkillsReturns.SkillStates.Commando;
using UnityEngine;
using UnityEngine.AddressableAssets;
using SneedHooks;

namespace SkillsReturns.SkillSetup.Commando
{
    public class CommandoKnife : SkillBase<CommandoKnife>
    {
        public override string SkillName => "Commando - Combat Knife";

        public override string SkillLangTokenName => "COMMANDO_SECONDARY_SKILLSRETURNS_SLASHKNIFE_NAME";

        public override string SkillLangTokenDesc => "COMMANDO_SECONDARY_SKILLSRETURNS_SLASHKNIFE_DESCRIPTION";

        public override SkillFamily SkillFamily => Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Commando/CommandoBodySecondaryFamily.asset").WaitForCompletion();

        public static DamageAPI.ModdedDamageType knifeDamageType;
        public static BuffDef knifeDebuff;
        
        protected override void CreateAssets()
        {
            if (knifeDebuff) return;
            knifeDamageType = DamageAPI.ReserveDamageType();
            knifeDebuff = SkillsReturns.Utilities.CreateBuffDef("CommandoKnifeDebuff", false, false, true, new Color32(81, 0, 0, 255), Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Bandit2/texBuffSuperBleedingIcon.tif").WaitForCompletion());
           
        }

        protected override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += ApplyKnifeDebuff;

            if (ModCompat.LinearDamage.pluginLoaded)
            {
                SneedHooks.ModifyFinalDamage.ModifyFinalDamageActions += ModifyFinalDamage_Additive;
            }
            else
            {
                SneedHooks.ModifyFinalDamage.ModifyFinalDamageActions += ModifyFinalDamage;
            }
        }

        private void ModifyFinalDamage(ModifyFinalDamage.DamageModifierArgs damageModifierArgs, DamageInfo damageInfo, HealthComponent victim, CharacterBody victimBody)
        {
            if (victimBody.HasBuff(knifeDebuff))
            {
                damageModifierArgs.damageMultFinal *= 1.5f;
                if (damageInfo.damageColorIndex == DamageColorIndex.Default)
                {
                    damageInfo.damageColorIndex = DamageColorIndex.SuperBleed;
                }
            }
        }

        private void ModifyFinalDamage_Additive(ModifyFinalDamage.DamageModifierArgs damageModifierArgs, DamageInfo damageInfo, HealthComponent victim, CharacterBody victimBody)
        {
            if (victimBody.HasBuff(knifeDebuff))
            {
                damageModifierArgs.damageMultAdd += 0.5f;
                if (damageInfo.damageColorIndex == DamageColorIndex.Default)
                {
                    damageInfo.damageColorIndex = DamageColorIndex.SuperBleed;
                }
            }
        }

        private void ApplyKnifeDebuff(DamageReport report)
        {
            if (!report.victimBody) return;
            if (report.damageInfo.rejected) return;
            if (!report.damageInfo.HasModdedDamageType(knifeDamageType)) return;

            report.victimBody.AddTimedBuff(knifeDebuff, 5f);
        }

        protected override void CreateSkillDef()
        {
            base.CreateSkillDef();
            skillDef.activationState = new SerializableEntityStateType(typeof(SlashKnife));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 3f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = false;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Skill;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("CombatKnifeIcon");
            skillDef.keywordTokens = new string[] { "KEYWORD_STUNNING" };

            LanguageAPI.Add(SkillLangTokenName, "Combat Knife");
            LanguageAPI.Add(SkillLangTokenDesc, "Slash enemies for <style=cIsDamage>360% damage</style>, wounding and <style=cIsDamage>stunning</style> enemies. Wounded enemies take <style=cIsDamage>50% more damage</style>.");
        }

        protected override void RegisterStates()
        {
            ContentAddition.AddEntityState(typeof(SlashKnife), out bool wasAdded);
        }
    }
}
