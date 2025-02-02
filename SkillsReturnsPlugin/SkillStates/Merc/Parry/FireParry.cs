using EntityStates;
using EntityStates.AffixVoid;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace SkillsReturns.SkillStates.Merc.Parry
{
    public class FireParry : BaseState
    {
        public static float baseDuration = 0.5f;
        public static float parryDuration = 1f/3f;
        public static float parryHitboxScale = 4f;  //Multiply your hitbox size for easier parrying

        public static BuffDef parryBuff;

        public static NetworkSoundEventDef soundSlashStandard;
        public static NetworkSoundEventDef soundSlashSuccessful;
        public static NetworkSoundEventDef soundDeflect;
        public static GameObject startEffect;
        public static GameObject impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab").WaitForCompletion();
        public static GameObject slashEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlashWhirlwind.prefab").WaitForCompletion();

        public static float damageCoefficient = 5f;
        public static float perfectDamageCoefficient = 15f;

        public static float radius = 12f;
        public static float perfectRadius = 20f;

        private float internalDeflectSoundCooldown = 0f;
        private int parryCount = 0;
        private bool attackFired = false;
        private float duration;

        private bool modifiedHitbox = false;
        private Vector3 originalHitboxScale;

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("Play_SkillsReturns_Merc_Parry_Release", base.gameObject);
            duration = baseDuration / attackSpeedStat;

            PlayAnimation("FullBody, Override", "GroundLight2", "GroundLight.playbackRate", duration);
            if (startEffect) EffectManager.SimpleImpactEffect(startEffect, base.transform.position, Vector3.up, false);

            if (NetworkServer.active)
            {
                if (characterBody)
                {
                    characterBody.AddTimedBuff(parryBuff, 10f); //duration is arbitrary
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            ApplyHitboxModifier();

            if (NetworkServer.active && characterBody && !attackFired)
            {
                internalDeflectSoundCooldown -= GetDeltaTime();
                int currentParryCount = characterBody.GetBuffCount(parryBuff) - 1;
                if (currentParryCount > parryCount)
                {
                    parryCount = currentParryCount;
                    if (soundDeflect && internalDeflectSoundCooldown <= 0f)
                    {
                        internalDeflectSoundCooldown = 0.05f;
                        EffectManager.SimpleSoundEffect(soundDeflect.index, base.transform.position, true);
                    }
                }
            }

            if (fixedAge >= parryDuration && !attackFired)
            {
                FireAttack();
            }

            if (isAuthority && fixedAge >= (parryDuration + duration))
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            RemoveHitboxModifier();
            if (!attackFired) FireAttackServer();

            if (NetworkServer.active && characterBody)
            {
                characterBody.ClearTimedBuffs(parryBuff);
            }

            Animator animator = GetModelAnimator();
            if (animator)
            {
                int layerIndex = animator.GetLayerIndex("Impact");
                if (layerIndex >= 0)
                {
                    animator.SetLayerWeight(layerIndex, 3f);
                    PlayAnimation("Impact", "LightImpact");
                }
            }

            base.OnExit();
        }

        //Play the attack anims locally.
        private void FireAttack()
        {
            if (attackFired) return;
            if (NetworkServer.active) FireAttackServer();

            attackFired = true;
            PlayCrossfade("FullBody, Override", "WhirlwindGround", "Whirlwind.playbackRate", duration, 0.1f);
            if (slashEffect) EffectManager.SimpleMuzzleFlash(slashEffect, base.gameObject, "WhirlwindGround", false);
        }

        //Fire the actual attack. Only the server knows whether you actually parried.
        private void FireAttackServer()
        {
            if (!NetworkServer.active || attackFired) return;

            bool isParry = parryCount > 0;
            if (characterBody)
            {
                characterBody.ClearTimedBuffs(parryBuff);
                if (isParry)
                {
                    characterBody.AddTimedBuff(RoR2Content.Buffs.Immune, duration + 0.3f);
                }
            }

            NetworkSoundEventDef attackSound = isParry ? soundSlashSuccessful : soundSlashStandard;
            if (attackSound) EffectManager.SimpleSoundEffect(attackSound.index, base.transform.position, true);

            EffectIndex effectIndex = EffectIndex.Invalid;
            if (impactEffect)
            {
                EffectComponent ec = impactEffect.GetComponent<EffectComponent>();
                if (ec) effectIndex = ec.effectIndex;
            }

            float attackRadius = isParry ? perfectRadius : radius;
            DestroyProjectiles(attackRadius);

            new BlastAttack
            {
                position = base.transform.position,
                attacker = base.gameObject,
                inflictor = base.gameObject,
                radius = attackRadius,
                baseDamage = damageStat * (isParry ? perfectDamageCoefficient : damageCoefficient),
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                crit = RollCrit(),
                damageType = (DamageTypeCombo) (isParry ? DamageType.ApplyMercExpose | DamageType.Stun1s : DamageType.Stun1s) | DamageSource.Secondary,
                procChainMask = default,
                procCoefficient = 1f,
                falloffModel = BlastAttack.FalloffModel.None,
                teamIndex = GetTeam(),
                damageColorIndex = isParry ? DamageColorIndex.Sniper : DamageColorIndex.Default,
                baseForce = 0f,
                bonusForce = Vector3.zero,
                impactEffect = effectIndex
            }.Fire();

            if (isParry)
            {
                //Taken from Nuxlar's mod
                EffectManager.SimpleImpactEffect(EntityStates.Merc.Evis.hitEffectPrefab, this.characterBody.corePosition, Vector3.one, true);
                EffectManager.SimpleImpactEffect(EntityStates.Merc.Evis.hitEffectPrefab, this.characterBody.corePosition, Vector3.zero, true);
                EffectManager.SimpleImpactEffect(EntityStates.Merc.Evis.hitEffectPrefab, this.characterBody.corePosition, Vector3.left, true);
                EffectManager.SimpleImpactEffect(EntityStates.Merc.Evis.hitEffectPrefab, this.characterBody.corePosition, Vector3.right, true);
            }
        }

        //Increase hitbox size to make it easier to parry melee enemies.
        private void ApplyHitboxModifier()
        {
            if (modifiedHitbox || !characterBody || !characterBody.mainHurtBox) return;
            modifiedHitbox = true;
            originalHitboxScale = characterBody.mainHurtBox.transform.localScale;
            characterBody.mainHurtBox.transform.localScale *= parryHitboxScale;
        }

        //Undo hitbox size modifier.
        private void RemoveHitboxModifier()
        {
            if (!modifiedHitbox) return;
            if (characterBody && characterBody.mainHurtBox)
            {
                characterBody.mainHurtBox.transform.localScale = originalHitboxScale;
            }
        }

        private void DestroyProjectiles(float radius)
        {
            float radiusSqr = radius * radius;
            TeamIndex teamIndex = GetTeam();
            List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
            List<ProjectileController> deleteList = [];

            foreach (ProjectileController pc in instancesList)
            {
                if (pc.cannotBeDeleted || pc.teamFilter.teamIndex == teamIndex || (pc.transform.position - base.transform.position).sqrMagnitude > radiusSqr) continue;
                ProjectileSimple ps = pc.gameObject.GetComponent<ProjectileSimple>();
                ProjectileCharacterController pcc = pc.gameObject.GetComponent<ProjectileCharacterController>();
                if ((!ps || (ps && ps.desiredForwardSpeed == 0f)) && !pcc) continue;

                deleteList.Add(pc);
            }

            foreach (ProjectileController pc in deleteList)
            {
                UnityEngine.Object.Destroy(pc.gameObject);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
