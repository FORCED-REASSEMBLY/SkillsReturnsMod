using EntityStates;
using RoR2;
using R2API;
using RoR2.Skills;
using System;
using UnityEngine;
using EntityStates.Captain.Weapon;
using UnityEngine.AddressableAssets;
using RoR2.UI;

namespace SkillsReturns.SkillStates.Huntress
{
    internal class HuntressBowCharge : EntityStates.BaseSkillState
    {
        public static float baseMinDuration = 0f;
        public static float baseChargeDuration = 1.5f;
        private float minDuration;
        public float chargeDuration;
        public float spread;
        private bool playedChargeSound = false;
        public static GameObject ChargeEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/ImpactLoaderFistSmall.prefab").WaitForCompletion();
        public static GameObject crosshairOverridePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
        private CrosshairUtils.OverrideRequest crosshairOverrideRequest;
        private Animator animator;

        private float origPlaybackRate;

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("Play_huntress_m1_ready", gameObject);    
            chargeDuration = HuntressBowCharge.baseChargeDuration / this.attackSpeedStat;
            minDuration = HuntressBowCharge.baseMinDuration / this.attackSpeedStat;
            crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
            StartAimMode(GetAimRay(), 2f, false);

            //Hacky: Multiply duration. Freeze animation at full charge.
            base.PlayCrossfade("Gesture, Override", "FireSeekingShot", "FireSeekingShot.playbackRate", chargeDuration * 2.2f, chargeDuration * 0.2f / this.attackSpeedStat);
            base.PlayCrossfade("Gesture, Additive", "FireSeekingShot", "FireSeekingShot.playbackRate", chargeDuration * 2.2f, chargeDuration * 0.2f / this.attackSpeedStat);

            animator = base.GetModelAnimator();
            if (animator)
            {
                origPlaybackRate = animator.GetFloat("FireSeekingShot.playbackRate");
            }
        }

        public override void OnExit()
        {
            if (crosshairOverrideRequest != null) crosshairOverrideRequest.Dispose();

            //Reset anim state so you don't get locked if you get frozen.
            base.PlayAnimation("Gesture, Override", "BufferEmpty");
            base.PlayAnimation("Gesture, Additive", "BufferEmpty");
            if (animator)
            {
                animator.SetFloat("FireSeekingShot.playbackRate", origPlaybackRate);
            }
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            StartAimMode(GetAimRay(), 2f, false);
            spread = Mathf.Lerp(1f, 0f, CalculateChargePercent());
            characterBody.SetSpreadBloom(spread, false);

            if (!playedChargeSound && CalculateChargePercent() >= 1f)
            {
                playedChargeSound = true;
                EffectManager.SimpleMuzzleFlash(ChargeEffectPrefab, gameObject, "Muzzle", false);
                Util.PlaySound("Play_SkillsReturns_Huntress_ChargeBow_Ready", base.gameObject);

                //Lock animation at full charge.
                if (animator)
                {
                    animator.SetFloat("FireSeekingShot.playbackRate", 0f);
                }
            }

            if (isAuthority)
            {
                bool shouldExit = base.inputBank && !base.inputBank.skill1.down && base.fixedAge >= minDuration;
                if (shouldExit)
                {
                    this.outer.SetNextState(new HuntressChargeArrowFire()
                    {
                        chargeFraction = CalculateChargePercent()
                    });
                    return;
                }
            }
        }

        public float CalculateChargePercent()
        {
           
            return Mathf.Lerp(0f, 1f, fixedAge / chargeDuration);          
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    } 
}
