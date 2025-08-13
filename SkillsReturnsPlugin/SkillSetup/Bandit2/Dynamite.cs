using EntityStates;
using HG;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using SkillsReturns.SkillSetup.Bandit2.Components.Dynamite;
using SkillsReturns.SkillStates.Bandit2.Dynamite;
using SkillsReturns.SkillStates.Bandit2.FlashBang;
using SkillsReturns.SkillStates.Merc.Parry;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace SkillsReturns.SkillSetup.Bandit2
{
    public class Dynamite : SkillBase<Dynamite>
    {
        public override string SkillName => "Bandit - Dynamite Toss";

        public override string SkillLangTokenName => "BANDIT2_SECONDARY_SKILLSRETURNS_DYNAMITE_NAME";

        public override string SkillLangTokenDesc => "BANDIT2_SECONDARY_SKILLSRETURNS_DYNAMITE_DESCRIPTION";

        public override SkillFamily SkillFamily => Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Bandit2/Bandit2BodySecondaryFamily.asset").WaitForCompletion();

        protected override void CreateSkillDef()
        {
            base.CreateSkillDef();
            skillDef.activationState = new SerializableEntityStateType(typeof(ThrowDynamite));
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
            skillDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("DynamiteIcon");
            skillDef.keywordTokens = new string[] { "KEYWORD_IGNITE" };

            LanguageAPI.Add(SkillLangTokenName, "Dynamite Toss");
            LanguageAPI.Add(SkillLangTokenDesc, "<style=cIsDamage>Ignite</style>. Throw a dynamite bundle in an arc for <style=cIsDamage>420% damage</style>. Deals <style=cIsDamage>double damage</style> when shot.");
        }

        protected override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool isDynamite = false;
            bool banditAttacker = false;
            if (NetworkServer.active && self.body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless))
            {
                AssignDynamiteTeamFilter ad = self.gameObject.GetComponent<AssignDynamiteTeamFilter>();
                if (ad)
                {
                    isDynamite = ad;

                    CharacterBody attackerCB = null;
                    if (damageInfo.attacker)
                    {
                        attackerCB = damageInfo.attacker.GetComponent<CharacterBody>();
                        if (attackerCB)
                        {
                            banditAttacker = attackerCB.bodyIndex == BodyCatalog.FindBodyIndex("Bandit2Body");
                        }
                    }
                }

                if (isDynamite)
                {
                    if  (banditAttacker && !ad.fired && !damageInfo.damageType.damageType.HasFlag(DamageType.AOE) && damageInfo.damageType.IsDamageSourceSkillBased && damageInfo.procCoefficient > 0f)
                    {
                        ad.fired = true;
                        damageInfo.procCoefficient = 0f;
                        damageInfo.crit = true;
                        ProjectileImpactExplosion pie = self.gameObject.GetComponent<ProjectileImpactExplosion>();
                        if (pie)
                        {
                            pie.blastRadius *= 2f;
                        }

                        ProjectileDamage pd = self.gameObject.GetComponent<ProjectileDamage>();
                        if (pd)
                        {
                            pd.damage *= 2f;

                            if (damageInfo.damageType.damageSource == DamageSource.Special)
                            {
                                pd.damage *= 1.5f;

                                //Transfer damageType to explosion
                                pd.damageType = damageInfo.damageType;
                                pd.damageType.damageSource = DamageSource.Secondary;
                                pd.damageType.damageType |= DamageType.IgniteOnHit | DamageType.AOE;
                                pd.damageType.damageType &= ~DamageType.BonusToLowHealth;
                                damageInfo.damageType = DamageTypeCombo.GenericSpecial;

                                DynamiteNetworkCommands nc = damageInfo.attacker.GetComponent<DynamiteNetworkCommands>();
                                if (nc) nc.RpcResetSpecialCooldown();
                            }
                        }
                    }
                    else
                    {
                        damageInfo.rejected = true;
                    }
                }
            }

            orig(self, damageInfo);
        }

        protected override void CreateAssets()
        {
            //This is for resetting special skill on dynamite shot
            GameObject bandit2Object = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2Body.prefab").WaitForCompletion();
            bandit2Object.AddComponent<DynamiteNetworkCommands>();

            GameObject dynamiteExplosionEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFX.prefab").WaitForCompletion().InstantiateClone("SkillsReturnsDynamiteExplosion", false);
            EffectComponent ec = dynamiteExplosionEffect.GetComponent<EffectComponent>();
            ec.soundName = "Play_SkillsReturns_Bandit_Dynamite_Explo";
            R2API.ContentAddition.AddEffect(dynamiteExplosionEffect);

            GameObject dynamiteGhost = Assets.mainAssetBundle.LoadAsset<GameObject>("DynamiteBundle");
            dynamiteGhost.layer = LayerIndex.noCollision.mask;
            dynamiteGhost.AddComponent<ProjectileGhostController>();

            //Clone this because the trigger-based detonation bypasses the collision jank
            GameObject dynamiteProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageLightningboltBasic.prefab").WaitForCompletion().InstantiateClone("SkillsReturnsDynamiteProjectile", true);

            ProjectileController pc = dynamiteProjectile.GetComponent<ProjectileController>();
            pc.allowPrediction = false;
            pc.ghostPrefab = dynamiteGhost;

            UnityEngine.Object.Destroy(dynamiteProjectile.GetComponent<MineProximityDetonator>());

            dynamiteProjectile.AddComponent<DynamiteRotation>();

            ProjectileSimple ps = dynamiteProjectile.GetComponent<ProjectileSimple>();
            ps.desiredForwardSpeed = 60f;
            ps.lifetime = 10f;

            Rigidbody rb = dynamiteProjectile.GetComponent<Rigidbody>();
            rb.useGravity = true;

            ProjectileImpactExplosion pie = dynamiteProjectile.GetComponent<ProjectileImpactExplosion>();
            pie.blastRadius = 10f;
            pie.impactEffect = dynamiteExplosionEffect;
            pie.destroyOnEnemy = true;
            pie.destroyOnWorld = false;
            pie.falloffModel = RoR2.BlastAttack.FalloffModel.None;
            pie.blastProcCoefficient = 1f;
            pie.timerAfterImpact = true;
            pie.lifetimeAfterImpact = 1f;

            TeamComponent tc = dynamiteProjectile.AddComponent<TeamComponent>();
            tc.hideAllyCardDisplay = true;

            dynamiteProjectile.AddComponent<SkillLocator>();
            CharacterBody cb = dynamiteProjectile.AddComponent<CharacterBody>();
            cb.rootMotionInMainState = false;
            cb.bodyFlags = CharacterBody.BodyFlags.Masterless;
            cb.baseMaxHealth = 1f;
            cb.baseCrit = 0f;
            cb.baseAcceleration = 0f;
            cb.baseArmor = 0f;
            cb.baseAttackSpeed = 0f;
            cb.baseDamage = 0f;
            cb.baseJumpCount = 0;
            cb.baseJumpPower = 0f;
            cb.baseMoveSpeed = 0f;
            cb.baseMaxShield = 0f;
            cb.baseRegen = 0f;
            cb.autoCalculateLevelStats = true;
            cb.levelArmor = 0f;
            cb.levelAttackSpeed = 0f;
            cb.levelCrit = 0f;
            cb.levelDamage = 0f;
            cb.levelJumpPower = 0f;
            cb.levelMaxHealth = 0f;
            cb.levelMaxShield = 0f;
            cb.levelMoveSpeed = 0f;
            cb.levelRegen = 0f;
            cb.hullClassification = HullClassification.Human;

            HealthComponent hc = dynamiteProjectile.AddComponent<HealthComponent>();
            hc.globalDeathEventChanceCoefficient = 0f;
            hc.body = cb;
            pie.projectileHealthComponent = hc;

            //Order of operations when adding this component is important, or else the whole thing just breaks.
            dynamiteProjectile.AddComponent<AssignDynamiteTeamFilter>();
            AddDynamiteHurtbox(dynamiteProjectile);

            R2API.ContentAddition.AddBody(dynamiteProjectile);
            R2API.ContentAddition.AddProjectile(dynamiteProjectile);
            ThrowDynamite.projectilePrefab = dynamiteProjectile;
        }

        private void AddDynamiteHurtbox(GameObject go)
        {
            GameObject hbObject = new GameObject();
            hbObject.transform.parent = go.transform;
            //GameObject hbObject = go;

            hbObject.layer = LayerIndex.entityPrecise.intVal;
            SphereCollider goCollider = hbObject.AddComponent<SphereCollider>();
            goCollider.radius = 1f;

            HurtBoxGroup goHurtBoxGroup = hbObject.AddComponent<HurtBoxGroup>();
            HurtBox goHurtBox = hbObject.AddComponent<HurtBox>();
            goHurtBox.isBullseye = false;
            goHurtBox.healthComponent = go.GetComponent<HealthComponent>();
            goHurtBox.damageModifier = HurtBox.DamageModifier.Normal;
            goHurtBox.hurtBoxGroup = goHurtBoxGroup;
            goHurtBox.indexInGroup = 0;

            HurtBox[] goHurtBoxArray = new HurtBox[]
            {
                goHurtBox
            };

            goHurtBoxGroup.bullseyeCount = 0;
            goHurtBoxGroup.hurtBoxes = goHurtBoxArray;
            goHurtBoxGroup.mainHurtBox = goHurtBox;

            DisableCollisionsBetweenColliders dc = go.AddComponent<DisableCollisionsBetweenColliders>();
            dc.collidersA = go.GetComponentsInChildren<Collider>();
            dc.collidersB = hbObject.GetComponents<Collider>();
        }

        protected override void RegisterStates()
        {
            ContentAddition.AddEntityState(typeof(ThrowDynamite), out bool wasAdded);
        }
    }
}
