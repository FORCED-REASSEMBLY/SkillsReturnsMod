using EntityStates;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.Captain.Weapon;
using System;
using System.Collections.Generic;
using System.Text;
using EntityStates.Toolbot;

namespace SkillsReturns.SkillStates.Engineer
{
    public class EngiMortarFire : BaseSkillState
    {
        public static float baseDuration = 0.2f;
        private float duration;
        public static GameObject shotgunHitsparkEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Commando/HitsparkCommandoBarrage.prefab").WaitForCompletion();
        public static GameObject shotgunBulletEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/TracerCommandoDefault.prefab").WaitForCompletion();

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            StartAimMode(aimRay, 2f, false);
            Util.PlaySound("Play_MULT_m1_grenade_launcher_shoot", gameObject);

            if (isAuthority)
            {

                BulletAttack ba = new BulletAttack
                {
                    owner = gameObject,
                    damage = damageStat * 0.8f,
                    procCoefficient = 1f,
                    force = 1f,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    maxDistance = 100f,
                    radius = 1f,
                    bulletCount = 1U,
                    minSpread = 0f,
                    maxSpread = 3f,
                    filterCallback = BulletAttack.defaultFilterCallback,
                    hitCallback = BulletAttack.defaultHitCallback,
                    stopperMask = LayerIndex.enemyBody.collisionMask,
                    isCrit = base.RollCrit(),
                    tracerEffectPrefab = shotgunBulletEffectPrefab,
                    hitEffectPrefab = shotgunHitsparkEffectPrefab,

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
