using EntityStates;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;
using RoR2.Projectile;

namespace SkillsReturns.SkillStates.Bandit2.FlashBang
{
    public class ThrowFlashbang : BaseSkillState
    {
        public static GameObject projectilePrefab;

        public static float baseDuration = 0.5f;
        private float duration;
        public static float projectileSpeed = 80f;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            base.PlayAnimation("Gesture, Additive", "SlashBlade", "SlashBlade.playbackRate", duration);
            base.PlayAnimation("Gesture, Override", "SlashBlade", "SlashBlade.playbackRate", duration);
            Util.PlaySound("Play_commando_M2_grenade_throw", gameObject);


            if (base.isAuthority)
            {
                if (base.characterMotor && !base.characterMotor.isGrounded)
                {
                    base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, Mathf.Max(base.characterMotor.velocity.y, 6), base.characterMotor.velocity.z);
                }

                Ray aimRay = base.GetAimRay();
                FireProjectileInfo info = new FireProjectileInfo()
                {
                    crit = false,
                    damage = 0f,
                    damageColorIndex = DamageColorIndex.Default,
                    force = 0f,
                    owner = base.gameObject,
                    position = aimRay.origin,
                    procChainMask = default,
                    projectilePrefab = ThrowFlashbang.projectilePrefab,
                    rotation = Quaternion.LookRotation(base.GetAimRay().direction),
                    useFuseOverride = false,
                    useSpeedOverride = true,
                    speedOverride = ThrowFlashbang.projectileSpeed,
                    target = null
                };
                ProjectileManager.instance.FireProjectile(info);
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


