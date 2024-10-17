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
        public static GameObject engiMortarProjectilePrefab;
        public float damageCoefficient = 1f;
        public float force = 100f;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            StartAimMode(aimRay, 2f, false);
            Util.PlaySound("Play_MULT_m1_grenade_launcher_shoot", gameObject);

            if (isAuthority)
            {
                Vector3 aimDirection = aimRay.direction;
                aimDirection.y = 0f;
                aimDirection.Normalize();
                aimDirection.y = 0.5f;
                aimDirection.Normalize();
                ProjectileManager.instance.FireProjectile(engiMortarProjectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimDirection), gameObject,
                damageStat * damageCoefficient, force, Util.CheckRoll(critStat, characterBody.master), DamageColorIndex.Default, null, -1f);
                
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
