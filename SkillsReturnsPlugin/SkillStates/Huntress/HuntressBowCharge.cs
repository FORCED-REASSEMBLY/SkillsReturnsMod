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

                if (!playedChargeSound && CalculateChargePercent() >= 1f)
                {
                    {
                        EffectManager.SimpleMuzzleFlash(ChargeEffectPrefab, gameObject, "Muzzle", false);
                    }
                    Util.PlaySound("Play_SkillsReturns_Huntress_ChargeBow_Ready", base.gameObject);
                    playedChargeSound = true;
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
                spread = Mathf.Lerp(1f, 0f, CalculateChargePercent());
                characterBody.SetSpreadBloom(spread, false);
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
