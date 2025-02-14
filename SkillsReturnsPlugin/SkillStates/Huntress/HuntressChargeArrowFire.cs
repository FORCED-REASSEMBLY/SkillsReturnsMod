using EntityStates;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;
using EntityStates.Huntress.HuntressWeapon;
using RoR2.UI;
using RoR2.Projectile;

namespace SkillsReturns.SkillStates.Huntress
{
    internal class HuntressChargeArrowFire : EntityStates.BaseSkillState
    {
        
        public static float baseDuration = 0.5f;
        private float duration;
        public float chargeFraction;
        public static GameObject ProjectilePrefab;
        public float minForce = 1000;
        public float maxForce = 2000;   
        public static float minDamageCoefficient = 2f;
        public static float maxDamageCoefficient = 10f;
        public static GameObject crosshairOverridePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressSnipeCrosshair.prefab").WaitForCompletion();
        private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            StartAimMode(aimRay, 2f, false);
            Util.PlaySound("Play_huntress_R_snipe_shoot", base.gameObject);
            crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);

            

            if (isAuthority)
            {
                Vector3 aimDirection = aimRay.direction;
                ProjectileManager.instance.FireProjectile(ProjectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimDirection), gameObject,
                damageStat * Mathf.Lerp(minDamageCoefficient, maxDamageCoefficient, chargeFraction), Mathf.Lerp(minForce, maxForce, chargeFraction), base.RollCrit(), DamageColorIndex.Default, null, 500f);
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

