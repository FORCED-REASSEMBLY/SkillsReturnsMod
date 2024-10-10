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
using RoR2.Projectile;

namespace SkillsReturns.SkillStates.Engineer
{
    public class EngiMortarFire : BaseSkillState
    {
        public static float baseDuration = 0.2f;
        private float duration;
        public static GameObject shotgunHitsparkEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Commando/HitsparkCommandoBarrage.prefab").WaitForCompletion();
        public static GameObject shotgunBulletEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/TracerCommandoDefault.prefab").WaitForCompletion();
        public static GameObject engiMortarProjectilePrefab;
        public float damageCoefficient = 0.8f;
        public float force = 100f;
        public float SpeedOverride = 5f;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            StartAimMode(aimRay, 2f, false);
            Util.PlaySound("Play_MULT_m1_grenade_launcher_shoot", gameObject);


            if (isAuthority)
            {
                
                ProjectileManager.instance.FireProjectile(engiMortarProjectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), gameObject,
                damageStat * damageCoefficient, force, Util.CheckRoll(critStat, characterBody.master), DamageColorIndex.Default, null, -1f);
                Debug.Log("projectile prefab activation debug message");
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
