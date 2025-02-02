using EntityStates;
using RoR2;
using R2API;
using RoR2.Skills;
using SkillsReturns.SkillStates.Huntress;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using SkillsReturns.SharedHooks;

namespace SkillsReturns.SkillSetup.Huntress
{
    public class HuntressChargePrimary : SkillBase<HuntressChargePrimary>
    {
        public override string SkillName => "Huntress - Pierce";

        public override string SkillLangTokenName => "HUNTRESS_PRIMARY_SKILLSRETURNS_CHARGEBOW_NAME";

        public override string SkillLangTokenDesc => "HUNTRESS_PRIMARY_SKILLSRETURNS_CHARGEBOW_DESCRIPTION";

        public override SkillFamily SkillFamily => Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Huntress/HuntressBodyPrimaryFamily.asset").WaitForCompletion();

        protected override void CreateSkillDef()
        {
            base.CreateSkillDef();
            skillDef.activationState = new SerializableEntityStateType(typeof(HuntressBowCharge));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 0f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = true;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Skill;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 0;
            skillDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("CombatKnifeIcon");

            LanguageAPI.Add(SkillLangTokenName, "Pierce");
            LanguageAPI.Add(SkillLangTokenDesc, "<style=cIsDamage>Slow down</style> and charge up a <style=cIsDamage>Piercing</style> arrow for <style=cIsDamage>200%-1000%</style> damage.");
        }

        protected override void RegisterStates()
        {
            ContentAddition.AddEntityState(typeof(HuntressBowCharge), out bool wasAdded);
            ContentAddition.AddEntityState(typeof(HuntressChargeArrowFire), out wasAdded);
        }
    }
}