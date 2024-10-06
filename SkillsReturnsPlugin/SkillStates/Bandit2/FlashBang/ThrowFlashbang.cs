using EntityStates;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;
using RoR2.Projectile;

namespace SkillsReturns.SkillStates.Bandit2.FlashBang
{
    
    public class ThrowFlashbang : AimThrowableBase
    {

        public static GameObject smokeProjectilePrefab;//Uses this name because AimThrowableBase has projectilePrefab field. Statics are bad for inheritance, but we use them here for convenience since we don't have easy access to EntityStateConfigurations.
        public static GameObject aimEndpointVisualizerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressArrowRainIndicator.prefab").WaitForCompletion();
        public static GameObject aimArcVisualizerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BasicThrowableVisualizer.prefab").WaitForCompletion();

        public override void OnEnter()
        {
            //Normally these would be set via EntityStateConfiguration instead
            maxDistance = 48;
            rayRadius = 2f;
            arcVisualizerPrefab = aimArcVisualizerPrefab;
            projectilePrefab = smokeProjectilePrefab;
            endpointVisualizerPrefab = aimEndpointVisualizerPrefab;
            endpointVisualizerRadiusScale = 4f;
            setFuse = false;
            damageCoefficient = 0f;
            baseMinimumDuration = 0f;   //Can be fired as soon as you release the button
            projectileBaseSpeed = 80f;

            base.OnEnter();
        }

        //Play sounds/anims once projectile is thrown.
        //Actual projectile throwing is built into AimThrowableBase
        public override void OnExit()
        {
            base.PlayAnimation("Gesture, Additive", "SlashBlade", "SlashBlade.playbackRate", 0.5f);
            base.PlayAnimation("Gesture, Override", "SlashBlade", "SlashBlade.playbackRate", 0.5f);
            Util.PlaySound("Play_commando_M2_grenade_throw", gameObject);

            if (base.isAuthority && base.characterMotor && !base.characterMotor.isGrounded)
            {
                base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, Mathf.Max(base.characterMotor.velocity.y, 6), base.characterMotor.velocity.z);
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}