using EntityStates;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.Captain.Weapon;

namespace SkillsReturns.SkillStates.Commando
{
    public class SpecialShotgunBlast : BaseSkillState
    {
        public static float baseDuration = 0.5f;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            StartAimMode(aimRay, 2f, false);
            PlayAnimation("Gesture, Additive", "FireBarrage", "FireBarrage.playbackRate", duration);
            PlayAnimation("Gesture, Override", "FireBarrage", "FireBarrage.playbackRate", duration);
            Util.PlaySound(FireCaptainShotgun.wideSoundString, gameObject);

            if (isAuthority)
            {
                BulletAttack ba = new BulletAttack
                {
                    owner = gameObject,
                    damage = damageStat * 2f,
                    procCoefficient = 1f,
                    force = 1f,
                    damageType = DamageType.Stun1s,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,                   
                    maxDistance = 25f,
                    radius = 1f,
                    bulletCount = 6U,
                    minSpread = 5f,
                    maxSpread = 10f,
                    filterCallback = BulletAttack.defaultFilterCallback,
                    hitCallback = BulletAttack.defaultHitCallback,
                    stopperMask = LayerIndex.world.collisionMask

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

