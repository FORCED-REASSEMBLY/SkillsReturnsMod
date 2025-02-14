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
        public static float baseMinDuration = 0.5f;
        public static float baseChargeDuration = 1.5f;
        private float minDuration;
        public float chargeDuration;
        private bool playedChargeSound = false;
        public static GameObject crosshairOverridePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressSnipeCrosshair.prefab").WaitForCompletion();
        private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("Play_huntress_m1_ready", gameObject);
            chargeDuration = HuntressBowCharge.baseChargeDuration / this.attackSpeedStat;
            minDuration = HuntressBowCharge.baseMinDuration / this.attackSpeedStat;
            crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
        }

        public override void OnExit()
        {
            if (crosshairOverrideRequest != null) crosshairOverrideRequest.Dispose();
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                if (!playedChargeSound && CalculateChargePercent() > 1f)
                {
                    playedChargeSound = true;
                    Util.PlaySound("Play_huntress_m1_unready", base.gameObject);

                }

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
