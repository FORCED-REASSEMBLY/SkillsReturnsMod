using EntityStates;
using RoR2;
using UnityEngine;
//Since we are using effects from Commando's Barrage skill, we will also be using the associated namespace
//You can also use Addressables or LegacyResourcesAPI to load whichever effects you like
using EntityStates.Commando.CommandoWeapon;
using UnityEngine.UIElements;
using R2API;
using UnityEngine.AddressableAssets;

namespace SkillsReturns.CommandoKnifeStates
{
    public class SimpleBullet : BaseSkillState 
    { 
        public static GameObject biteEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Lemurian/LemurianBiteTrail.prefab").WaitForCompletion();
    
        public float baseDuration = 0.5f;
        private float duration;

        //OnEnter() runs once at the start of the skill
        //All we do here is create a BulletAttack and fire it
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / base.attackSpeedStat;
            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);
            base.PlayAnimation("Gesture, Additive", "ThrowGrenade", "FireFMJ.playbackRate", duration * 2f);
            base.PlayAnimation("Gesture, Override", "ThrowGrenade", "FireFMJ.playbackRate", duration * 2f);
            Util.PlaySound("Play_bandit2_m2_impact", base.gameObject);
            base.AddRecoil(-0.6f, 0.6f, -0.6f, 0.6f);
            if (biteEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(biteEffectPrefab, base.gameObject, "MuzzleRight", false);
            }

            if (base.isAuthority)

            {
                BlastAttack ba = new BlastAttack
                {
                    attacker = base.gameObject,
                    inflictor = base.gameObject,
                    procCoefficient = 1f,
                    baseDamage = damageStat * 3.6f,
                    position = base.characterBody.corePosition,
                    teamIndex = base.GetTeam(),
                    radius = 8,
                    falloffModel = BlastAttack.FalloffModel.None,
                    baseForce = 0f,
                    bonusForce = Vector3.zero,
                    damageType = DamageType.Stun1s,
                    attackerFiltering = AttackerFiltering.NeverHitSelf

                };
                ba.AddModdedDamageType(SkillsReturns.SkillsReturnsPlugin.CommandoKnifeDamage);
                var result = ba.Fire();

                if (base.characterMotor && !base.characterMotor.isGrounded && result.hitCount > 0)
                {
                    base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, Mathf.Max(base.characterMotor.velocity.y, 6), base.characterMotor.velocity.z);   //Bandit2 FireShiv Shorthop Velocity = 6
                }

            }
        }

        //This method runs once at the end
        //Here, we are doing nothing
        public override void OnExit()
        {
            base.OnExit();
        }

        //FixedUpdate() runs almost every frame of the skill
        //Here, we end the skill once it exceeds its intended duration
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        //GetMinimumInterruptPriority() returns the InterruptPriority required to interrupt this skill
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}