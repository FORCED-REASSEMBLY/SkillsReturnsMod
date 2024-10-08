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
using SkillsReturns.SkillStates.Engineer;


namespace SkillsReturns.SkillSetup.Engineer
{
    public class EngiMortarPrimary : SkillBase<EngiMortarPrimary>
    {
        public override string SkillName => "Engineer - Mortar Barrage";

        public override string SkillLangTokenName => "ENGI_PRIMARY_SKILLSRETURNS_MORTARS_NAME";

        public override string SkillLangTokenDesc => "ENGI_PRIMARY_SKILLSRETURNS_MORTARS_DESCRIPTION";

        public override SkillFamily SkillFamily => Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Engi/EngiBodyPrimaryFamily.asset").WaitForCompletion();

        public SkillDef scepterDef;

        protected override void CreateSkillDef()
        {
            base.CreateSkillDef();
            skillDef.activationState = new SerializableEntityStateType(typeof(EngiMortarFire));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 0f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = false;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Skill;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 0;
            skillDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("PointBlankIcon");

            LanguageAPI.Add(SkillLangTokenName, "Mortar Barrage");
            LanguageAPI.Add(SkillLangTokenDesc, "Launch mortar rounds in an arc for <style=cIsDamage>80% damage</style>.");
        }
        protected override void RegisterStates()
        {
            ContentAddition.AddEntityState(typeof(EngiMortarFire), out bool wasAdded);
        }
    }
}