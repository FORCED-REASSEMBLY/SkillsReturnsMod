using EntityStates;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;

namespace SkillsReturns.SkillStates.Commando
{
    public class SlashKnife : BaseSkillState
    {
        public static GameObject biteEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Lemurian/LemurianBiteTrail.prefab").WaitForCompletion();

        public static float baseDuration = 0.5f;
        private float duration;

        //OnEnter() runs once at the start of the skill
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            StartAimMode(aimRay, 2f, false);
            PlayAnimation("Gesture, Additive", "ThrowGrenade", "FireFMJ.playbackRate", duration * 2f);
            PlayAnimation("Gesture, Override", "ThrowGrenade", "FireFMJ.playbackRate", duration * 2f);
            Util.PlaySound("Play_bandit2_m2_impact", gameObject);
            AddRecoil(-0.6f, 0.6f, -0.6f, 0.6f);
            if (biteEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(biteEffectPrefab, gameObject, "MuzzleRight", false);
            }

            if (isAuthority)
            {
                BlastAttack ba = new BlastAttack
                {
                    attacker = gameObject,
                    inflictor = gameObject,
                    procCoefficient = 1f,
                    baseDamage = damageStat * 3.6f,
                    position = characterBody.corePosition,
                    teamIndex = GetTeam(),
                    radius = 5f,
                    falloffModel = BlastAttack.FalloffModel.None,
                    baseForce = 0f,
                    bonusForce = Vector3.zero,
                    damageType = DamageType.Stun1s,
                    attackerFiltering = AttackerFiltering.NeverHitSelf

                };
                ba.AddModdedDamageType(SkillSetup.Commando.CommandoKnife.knifeDamageType);
                var result = ba.Fire();

                if (characterMotor && !characterMotor.isGrounded && result.hitCount > 0)
                {
                    characterMotor.velocity = new Vector3(characterMotor.velocity.x, Mathf.Max(characterMotor.velocity.y, 6), characterMotor.velocity.z);   //Bandit2 FireShiv Shorthop Velocity = 6
                }
            }
        }

        //FixedUpdate() runs almost every frame of the skill
        //Here, we end the skill once it exceeds its intended duration
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        //GetMinimumInterruptPriority() returns the InterruptPriority required to interrupt this skill
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}