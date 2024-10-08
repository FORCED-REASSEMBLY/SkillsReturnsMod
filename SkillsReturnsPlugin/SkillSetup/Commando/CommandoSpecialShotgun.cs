using EntityStates;
using RoR2;
using R2API;
using RoR2.Skills;
using SkillsReturns.SkillStates.Commando;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using SkillsReturns.SharedHooks;


namespace SkillsReturns.SkillSetup.Commando
{
    public class CommandoSpecialShotgun : SkillBase<CommandoSpecialShotgun>
    {
        public override string SkillName => "Commando - Point Blank";

        public override string SkillLangTokenName => "COMMANDO_SPECIAL_SKILLSRETURNS_SHOTGUNBLAST_NAME";

        public override string SkillLangTokenDesc => "COMMANDO_SPECIAL_SKILLSRETURNS_SHOTGUNBLAST_DESCRIPTION";

        public override SkillFamily SkillFamily => Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Commando/CommandoBodySpecialFamily.asset").WaitForCompletion();

        public SkillDef scepterDef;

        protected override void CreateSkillDef()
        {
            base.CreateSkillDef();
            skillDef.activationState = new SerializableEntityStateType(typeof(SpecialShotgunBlast));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 6f;
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
            skillDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("PointBlankIcon");

            //Add Stunning Keyword
            skillDef.keywordTokens = new string[] { "KEYWORD_STUNNING" };

            LanguageAPI.Add(SkillLangTokenName, "Point-Blank");
            LanguageAPI.Add(SkillLangTokenDesc, "<style=cIsDamage>Stunning.</style> Fire a <style=cIsDamage>piercing</style> shotgun blast for <style=cIsDamage>6x200% damage</style>.");

            if (ModCompat.AncientScepterCompat.pluginLoaded) BuildScepterSkill();
        }

        private void BuildScepterSkill()
        {
            scepterDef = ScriptableObject.CreateInstance<SkillDef>();
            scepterDef.skillName = SkillLangTokenName+"_SCEPTER";
            scepterDef.skillNameToken = SkillLangTokenName + "_SCEPTER";
            scepterDef.skillDescriptionToken = SkillLangTokenDesc + "_SCEPTER";
            scepterDef.keywordTokens = new string[] { "KEYWORD_STUNNING" };
            (scepterDef as ScriptableObject).name = SkillLangTokenName;

            //Note that it's possible for Scepter Skills to fully change which EntityState is used.
            //Here, we're just doing the lazy way and adding a built-in Scepter check while reusing the SpecialShotgunBlast state.
            scepterDef.activationState = new SerializableEntityStateType(typeof(SpecialShotgunBlast));

            scepterDef.activationStateMachineName = skillDef.activationStateMachineName;
            scepterDef.baseMaxStock = skillDef.baseMaxStock;
            scepterDef.baseRechargeInterval = skillDef.baseRechargeInterval;
            scepterDef.beginSkillCooldownOnSkillEnd = skillDef.beginSkillCooldownOnSkillEnd;
            scepterDef.canceledFromSprinting = skillDef.canceledFromSprinting;
            scepterDef.cancelSprintingOnActivation = skillDef.cancelSprintingOnActivation;
            scepterDef.fullRestockOnAssign = skillDef.fullRestockOnAssign;
            scepterDef.interruptPriority = skillDef.interruptPriority;
            scepterDef.isCombatSkill = skillDef.isCombatSkill;
            scepterDef.mustKeyPress = skillDef.mustKeyPress;
            scepterDef.rechargeStock = skillDef.rechargeStock;
            scepterDef.requiredStock = skillDef.requiredStock;
            scepterDef.stockToConsume = skillDef.stockToConsume;
            scepterDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("PointBlankIcon");   //TODO: SCEPTER ICON

            //SkillBase automatically registers SkillBase.skillDef. This is a new skillDef so we need to manually register it.
            ContentAddition.AddSkillDef(scepterDef);

            //This has code to automatically check if Scepter is actually loaded.
            ModCompat.AncientScepterCompat.AddScepterSkill(scepterDef, "CommandoBody", skillDef);

            LanguageAPI.Add(SkillLangTokenName + "_SCEPTER", "Lead Shot");
            LanguageAPI.Add(SkillLangTokenDesc + "_SCEPTER", "<style=cIsDamage>Stunning.</style> Fire a <style=cIsDamage>piercing</style> shotgun blast for <style=cIsDamage>12x200% damage</style>.");
        }

        protected override void RegisterStates()
        {
            ContentAddition.AddEntityState(typeof(SpecialShotgunBlast), out bool wasAdded);
        }
    }
}



