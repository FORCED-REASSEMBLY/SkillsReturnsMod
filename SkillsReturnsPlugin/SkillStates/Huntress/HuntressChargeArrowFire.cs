using EntityStates;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;
using EntityStates.Huntress.HuntressWeapon;
using RoR2.UI;
using RoR2.Projectile;
using System;

namespace SkillsReturns.SkillStates.Huntress
{
    internal class HuntressChargeArrowFire : EntityStates.BaseSkillState
    {
        
        public static float baseDuration = 0.5f;
        private float duration;
        public float chargeFraction;
        public static GameObject ProjectilePrefab;
        public float minForce = 500;
        public float maxForce = 2000;   
        public static float minDamageCoefficient = 2f;
        public static float maxDamageCoefficient = 8f;
        public static GameObject crosshairOverridePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();

        public static NetworkSoundEventDef soundShoot;
        public static NetworkSoundEventDef soundShootCharged;

        private CrosshairUtils.OverrideRequest crosshairOverrideRequest;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            StartAimMode(aimRay, 2f, false);
            crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);

            if (isAuthority)
            {
                ProjectileManager.instance.FireProjectile(ProjectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), gameObject,
                damageStat * Mathf.Lerp(minDamageCoefficient, maxDamageCoefficient, chargeFraction), Mathf.Lerp(minForce, maxForce, chargeFraction), base.RollCrit(), DamageColorIndex.Default, null, 180f, DamageTypeCombo.GenericPrimary);

                if (chargeFraction >= 1f)
                {
                    if (soundShootCharged) EffectManager.SimpleSoundEffect(soundShootCharged.index, base.transform.position, true);
                }
                else
                {
                    if (soundShoot) EffectManager.SimpleSoundEffect(soundShoot.index, base.transform.position, true);
                }
            }

            base.PlayCrossfade("Gesture, Override", "FireSeekingShot", "FireSeekingShot.playbackRate", duration * 0.8f, duration * 0.2f/this.attackSpeedStat);
            base.PlayCrossfade("Gesture, Additive", "FireSeekingShot", "FireSeekingShot.playbackRate", duration * 0.8f, duration * 0.2f/this.attackSpeedStat);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
        
    }

    
}

