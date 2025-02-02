using EntityStates;
using RoR2;
using R2API;
using RoR2.Skills;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using SkillsReturns.SharedHooks;
using System.Net.NetworkInformation;
using EntityStates.Huntress;
using EntityStates.Huntress.HuntressWeapon;
using EntityStates.Captain.Weapon;

namespace SkillsReturns.SkillStates.Huntress
{
    internal class HuntressBowCharge : EntityStates.BaseSkillState
    {
        public static float baseMinDuration = 0.5f;
        public static float baseChargeDuration = 1.5f;
        private float minChargeDuration = 0.5f;
        private float maxChargeDuration = 3f;
        private float chargeDuration;
        private float charge;
        public float chargePercent;
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(FireCaptainShotgun.wideSoundString, gameObject);
            this.minChargeDuration = minChargeDuration / this.attackSpeedStat;
            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(minChargeDuration);
            }
            charge = 0f;
            chargePercent = 0f;
            chargeDuration = HuntressBowCharge.baseChargeDuration / this.attackSpeedStat;

        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(maxChargeDuration);
            }
            if (base.fixedAge > this.minChargeDuration && charge < chargeDuration)
            {
                Util.PlaySound("Play_bandit2_m2_impact", gameObject);
                chargePercent = Mathf.Max(0f, (charge - baseMinDuration) / (baseChargeDuration - baseMinDuration));
            }
            if (base.fixedAge >= this.minChargeDuration)
            {
                if (base.isAuthority && (base.inputBank && !base.inputBank.skill2.down))
                {
                    this.outer.SetNextState(new HuntressChargeArrowFire());
                    return;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
