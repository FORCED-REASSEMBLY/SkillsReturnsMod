using EntityStates;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.Captain.Weapon;
using System;
using System.Collections.Generic;
using System.Text;
using EntityStates.Toolbot;
using RoR2.Projectile;
using RoR2.Skills;

namespace SkillsReturns.SkillStates.Engineer
{
    public class EngiMortarFire : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        public static float baseDuration = 0.2f;
        private float duration;
        public static GameObject engiMortarProjectilePrefab;
        public static GameObject muzzleflashEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/MuzzleflashSmokeRing.prefab").WaitForCompletion();
        public float damageCoefficient = 1f;
        public float force = 100f;
        public static float upwardsAimFactor = 0.5f;

        private int step;   //controls which barrel to shoot from

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            StartAimMode(aimRay, 2f, false);
            Util.PlaySound("Play_SkillsReturns_Engi_Shoot", gameObject);

            //Play VFX
            //Copied from Engi's FireGrenades
            if (step % 2 == 0) 
            {
                //Left Muzzle
                EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, "MuzzleLeft", false);
                base.PlayCrossfade("Gesture Left Cannon, Additive", "FireGrenadeLeft", duration);
            }
            else
            {
                //Right Muzzle
                EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, "MuzzleRight", false);
                base.PlayCrossfade("Gesture Right Cannon, Additive", "FireGrenadeRight", duration);
            }

            if (isAuthority)
            {
                Vector3 aimDirection = aimRay.direction;
                aimDirection.y = 0f;
                aimDirection.Normalize();
                aimDirection.y = upwardsAimFactor;
                aimDirection.Normalize();
                ProjectileManager.instance.FireProjectile(engiMortarProjectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimDirection), gameObject,
                damageStat * damageCoefficient, force, base.RollCrit(), DamageColorIndex.Default, null, -1f);
            }

            //Add spread to give visual feedback. Doesn't actually affect trajectory.
            base.characterBody.AddSpreadBloom(0.3f);
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

        public void SetStep(int i)
        {
            step = i;
        }
    }
}
