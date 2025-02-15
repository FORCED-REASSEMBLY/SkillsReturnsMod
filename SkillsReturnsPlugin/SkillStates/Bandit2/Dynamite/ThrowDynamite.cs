using EntityStates;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SkillsReturns.SkillStates.Bandit2.Dynamite
{
    public class ThrowDynamite : BaseState
    {
        public static float damageCoefficient = 4.2f;
        public static GameObject projectilePrefab;
        public static float baseDuration = 0.5f;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            StartAimMode(aimRay, 2f, false);
            PlayAnimation("Gesture, Additive", "SlashBlade", "SlashBlade.playbackRate", this.duration);
            Util.PlaySound("Play_SkillsReturns_Bandit_Dynamite_Throw", base.gameObject);
            if (isAuthority)
            {
                if (characterMotor && !characterMotor.isGrounded)
                {
                    characterMotor.velocity = new Vector3(characterMotor.velocity.x, Mathf.Max(characterMotor.velocity.y, 6),characterMotor.velocity.z);   //Bandit2 FireShiv Shorthop Velocity = 6
                }

                ProjectileManager.instance.FireProjectile(projectilePrefab,
                    aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject,
                    damageStat * damageCoefficient, 0f, RollCrit(),
                    DamageColorIndex.Default, null, -1f,
                    (DamageTypeCombo) DamageType.IgniteOnHit | DamageSource.Secondary);
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
            if (inputBank && inputBank.skill2.down)
            {
                return InterruptPriority.PrioritySkill;
            }
            return InterruptPriority.Skill;
        }
    }
}
