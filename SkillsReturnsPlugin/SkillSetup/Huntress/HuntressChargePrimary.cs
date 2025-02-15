using EntityStates;
using RoR2;
using R2API;
using RoR2.Skills;
using SkillsReturns.SkillStates.Huntress;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.Projectile;
using SkillsReturns.SkillStates.Bandit2.Dynamite;

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
            skillDef.canceledFromSprinting = false;
            skillDef.cancelSprintingOnActivation = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Skill;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 0;
            skillDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("PierceIcon");
            skillDef.keywordTokens = ["KEYWORD_AGILE"];

            LanguageAPI.Add(SkillLangTokenName, "Pierce");
            LanguageAPI.Add(SkillLangTokenDesc, "<style=cIsUtility>Agile</style>. Charge up a <style=cIsDamage>piercing</style> arrow for <style=cIsDamage>200%-800%</style> damage.");
        }
        protected override void CreateAssets()
        {
            GameObject chargeArrowProjectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/FMJRamping.prefab").WaitForCompletion().InstantiateClone("SkillsReturnsHuntressChargeArrowProjectile", true);
            ContentAddition.AddProjectile(chargeArrowProjectilePrefab);

            ProjectileController pc = chargeArrowProjectilePrefab.GetComponent<ProjectileController>();
            pc.ghostPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Huntress/ArrowGhost.prefab").WaitForCompletion();
            pc.allowPrediction = true;

            ProjectileOverlapAttack poa = chargeArrowProjectilePrefab.GetComponent<ProjectileOverlapAttack>();
            poa.onServerHit = null;
            poa.impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/OmniImpactVFXHuntress.prefab").WaitForCompletion();

            ProjectileSimple ps = chargeArrowProjectilePrefab.GetComponent<ProjectileSimple>();
            ps.lifetime = 5f;
           
            R2API.ContentAddition.AddProjectile(chargeArrowProjectilePrefab);
            HuntressChargeArrowFire.ProjectilePrefab = chargeArrowProjectilePrefab;
            chargeArrowProjectilePrefab.transform.localScale = new Vector3(2, 2, 2);

        }
        protected override void RegisterStates()
        {
            ContentAddition.AddEntityState(typeof(HuntressBowCharge), out bool wasAdded);
            ContentAddition.AddEntityState(typeof(HuntressChargeArrowFire), out wasAdded);
        }
    }
}