using EntityStates;
using EntityStates.AffixVoid;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace SkillsReturns.SkillStates.Merc.Parry
{
    public class FireParry : BaseState
    {
        public static float baseDuration = 0.5f;
        public static float parryDuration = 1f/3f;
        public static float parryHitboxScale = 4f;  //Multiply your hitbox size for easier parrying

        public static BuffDef parryBuff;

        public static NetworkSoundEventDef soundSlashStandard;
        public static NetworkSoundEventDef soundSlashSuccessful;
        public static NetworkSoundEventDef soundDeflect;
        public static GameObject impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab").WaitForCompletion();

        public static float damageCoefficient = 5f;
        public static float perfectDamageCoefficient = 15f;

        public static float radius = 12f;
        public static float perfectRadius = 20f;

        private float internalDeflectSoundCooldown = 0f;
        private int parryCount = 0;
        private bool attackFired = false;
        private float duration;

        private bool modifiedHitbox = false;
        private Vector3 originalHitboxScale;

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("Play_SkillsReturns_Merc_Parry_Release", base.gameObject);
            duration = baseDuration / attackSpeedStat;

            if (NetworkServer.active)
            {
                if (characterBody)
                {
                    characterBody.AddTimedBuff(parryBuff, 10f); //duration is arbitrary
                }
            }

            if (isAuthority && characterMotor)
            {
                characterMotor.velocity = new Vector3(characterMotor.velocity.x, 0f, characterMotor.velocity.z);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            ApplyHitboxModifier();

            if (isAuthority && characterMotor)
            {
                characterMotor.velocity = new Vector3(characterMotor.velocity.x, 0f, characterMotor.velocity.z);
            }

            if (NetworkServer.active && characterBody && !attackFired)
            {
                internalDeflectSoundCooldown -= GetDeltaTime();
                int currentParryCount = characterBody.GetBuffCount(parryBuff) - 1;
                if (currentParryCount > parryCount)
                {
                    parryCount = currentParryCount;
                    if (soundDeflect && internalDeflectSoundCooldown <= 0f)
                    {
                        internalDeflectSoundCooldown = 0.05f;
                        EffectManager.SimpleSoundEffect(soundDeflect.index, base.transform.position, true);
                    }
                }
            }

            if (fixedAge >= parryDuration && !attackFired)
            {
                FireAttack();
            }

            if (isAuthority && fixedAge >= (parryDuration + duration))
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            if (!attackFired) FireAttackServer();
            RemoveHitboxModifier();
            //Remember to reset animation state
            base.OnExit();
        }

        //Play the attack anims locally.
        private void FireAttack()
        {
            if (NetworkServer.active) FireAttackServer();

            //Play animation here
        }

        //Fire the actual attack. Only the server knows whether you actually parried.
        private void FireAttackServer()
        {
            if (!NetworkServer.active || attackFired) return;
            attackFired = true;

            bool isParry = parryCount > 0;
            if (characterBody)
            {
                characterBody.ClearTimedBuffs(parryBuff);
                if (isParry)
                {
                    characterBody.AddTimedBuff(RoR2Content.Buffs.Immune, duration + 0.3f);
                }
            }

            NetworkSoundEventDef attackSound = isParry ? soundSlashSuccessful : soundSlashStandard;
            if (attackSound) EffectManager.SimpleSoundEffect(attackSound.index, base.transform.position, true);

            //Handle damage here

            EffectIndex effectIndex = EffectIndex.Invalid;
            if (impactEffect)
            {
                EffectComponent ec = impactEffect.GetComponent<EffectComponent>();
                if (ec) effectIndex = ec.effectIndex;
            }

            float attackRadius = isParry ? perfectRadius : radius;
            DestroyProjectiles(attackRadius);

            new BlastAttack
            {
                position = base.transform.position,
                attacker = base.gameObject,
                inflictor = base.gameObject,
                radius = attackRadius,
                baseDamage = damageStat * (isParry ? perfectDamageCoefficient : damageCoefficient),
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                crit = RollCrit(),
                damageType = (DamageTypeCombo) (isParry ? DamageType.ApplyMercExpose | DamageType.Stun1s : DamageType.Stun1s) | DamageSource.Secondary,
                procChainMask = default,
                procCoefficient = 1f,
                falloffModel = BlastAttack.FalloffModel.None,
                teamIndex = GetTeam(),
                damageColorIndex = isParry ? DamageColorIndex.Sniper : DamageColorIndex.Default,
                baseForce = 0f,
                bonusForce = Vector3.zero,
                impactEffect = effectIndex
            }.Fire();

            //Spawn VFX here
        }

        //Increase hitbox size to make it easier to parry melee enemies.
        private void ApplyHitboxModifier()
        {
            if (modifiedHitbox || !characterBody || !characterBody.mainHurtBox) return;
            modifiedHitbox = true;
            originalHitboxScale = characterBody.mainHurtBox.transform.localScale;
            characterBody.mainHurtBox.transform.localScale *= parryHitboxScale;
        }

        //Undo hitbox size modifier.
        private void RemoveHitboxModifier()
        {
            if (!modifiedHitbox) return;
            if (characterBody && characterBody.mainHurtBox)
            {
                characterBody.mainHurtBox.transform.localScale = originalHitboxScale;
            }
        }

        private void DestroyProjectiles(float radius)
        {
            float radiusSqr = radius * radius;
            TeamIndex teamIndex = GetTeam();
            List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
            List<ProjectileController> deleteList = [];

            foreach (ProjectileController pc in instancesList)
            {
                if (pc.cannotBeDeleted || pc.teamFilter.teamIndex == teamIndex || (pc.transform.position - base.transform.position).sqrMagnitude > radiusSqr) continue;
                ProjectileSimple ps = pc.gameObject.GetComponent<ProjectileSimple>();
                ProjectileCharacterController pcc = pc.gameObject.GetComponent<ProjectileCharacterController>();
                if ((!ps || (ps && ps.desiredForwardSpeed == 0f)) && !pcc) continue;

                deleteList.Add(pc);
            }

            foreach (ProjectileController pc in deleteList)
            {
                UnityEngine.Object.Destroy(pc.gameObject);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
