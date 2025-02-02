using EntityStates;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;
using EntityStates.Huntress.HuntressWeapon;

namespace SkillsReturns.SkillStates.Huntress
{
    internal class HuntressChargeArrowFire : EntityStates.BaseSkillState
    {
        public static GameObject arrowBulletEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/FlurryArrowOrbEffect.prefab").WaitForCompletion();
        public static GameObject arrowHitSparkEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/HitsparkBandit.prefab").WaitForCompletion();
        public static float baseDuration = 0.5f;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            StartAimMode(aimRay, 2f, false);
            Util.PlaySound(FireArrow.attackSoundString, base.gameObject);

            if (arrowHitSparkEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(arrowHitSparkEffectPrefab, gameObject, "MuzzleRight", false);
            }

            if (arrowBulletEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(arrowBulletEffectPrefab, gameObject, "MuzzleRight", false);
            }

            if (isAuthority)
            {
                BulletAttack ba = new BulletAttack
                {
                    owner = gameObject,
                    damage = damageStat * 10f,
                    procCoefficient = 1f,
                    force = 1f,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    maxDistance = 200f,
                    radius = 1f,
                    bulletCount = 1,
                    minSpread = 0f,
                    maxSpread = 0f,
                    filterCallback = BulletAttack.defaultFilterCallback,
                    hitCallback = BulletAttack.defaultHitCallback,
                    stopperMask = LayerIndex.world.collisionMask,
                    isCrit = base.RollCrit(),
                    tracerEffectPrefab = arrowBulletEffectPrefab,
                    hitEffectPrefab = arrowHitSparkEffectPrefab,

                }; ba.Fire();
            }
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

