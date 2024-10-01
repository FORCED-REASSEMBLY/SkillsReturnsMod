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

        public override SkillFamily SkillFamily => Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Commando/CommandoBodySecondaryFamily.asset").WaitForCompletion();

        protected override void CreateSkillDef()
        {
            base.CreateSkillDef();
            skillDef.activationState = new SerializableEntityStateType(typeof(SpecialShotgunBlastState));
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
            skillDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("CombatKnifeIcon");

            // We use LanguageAPI to add strings to the game, in the form of tokens
            // Please note that it is instead recommended that you use a language file.
            // More info in https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
            LanguageAPI.Add("COMMANDO_SPECIAL_SKILLSRETURNS_SHOTGUNBLAST_NAME", "Point-Blank");
            LanguageAPI.Add("COMMANDO_SPECIAL_SKILLSRETURNS_SHOTGUNBLAST_DESCRIPTION", "Charge up a Shotgun Blast for <style=cIsDamage>6x200% - 10x200% damage</style>.");

            protected override void RegisterStates()
        {
            ContentAddition.AddEntityState(typeof(SpecialShotgunBlastState), out bool wasAdded);
        }
    }
    }


}
