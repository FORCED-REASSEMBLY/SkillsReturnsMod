using EntityStates;
using RoR2;
using R2API;
using RoR2.Skills;
using SkillsReturns.SkillStates.Commando;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using SkillsReturns.SharedHooks;
using SkillsReturns.SkillStates.Engineer;
using System.Net.NetworkInformation;
using RoR2.UI;
using RoR2.Projectile;
using UnityEngine.ProBuilder;


namespace SkillsReturns.SkillSetup.Engineer
{
    public class EngiMortarPrimary : SkillBase<EngiMortarPrimary>
    {
        public override string SkillName => "Engineer - Mortar Barrage";

        public override string SkillLangTokenName => "ENGI_PRIMARY_SKILLSRETURNS_MORTARS_NAME";

        public override string SkillLangTokenDesc => "ENGI_PRIMARY_SKILLSRETURNS_MORTARS_DESCRIPTION";

        public override SkillFamily SkillFamily => Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Engi/EngiBodyPrimaryFamily.asset").WaitForCompletion();


        public SkillDef scepterDef;
      

        protected override void CreateSkillDef()
        {
            base.CreateSkillDef();
            skillDef.activationState = new SerializableEntityStateType(typeof(EngiMortarFire));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 0f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = false;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Skill;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 0;
            skillDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("MortarBarrageIcon");

            LanguageAPI.Add(SkillLangTokenName, "Mortar Barrage");
            LanguageAPI.Add(SkillLangTokenDesc, "Launch mortar rounds in an arc for <style=cIsDamage>100% damage</style>.");
        }

        protected override void CreateAssets()
        {

            GameObject projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ToolbotGrenadeLauncherProjectile.prefab").WaitForCompletion()
            .InstantiateClone("SkillsReturnsEngiMortarProjectile", true);
            ContentAddition.AddProjectile(projectilePrefab);
            EngiMortarFire.engiMortarProjectilePrefab = projectilePrefab;
            Rigidbody EngiMortarRigidBody = projectilePrefab.GetComponent<Rigidbody>();
            EngiMortarRigidBody.useGravity = true;
            EngiMortarRigidBody.mass = 1f;
            EngiMortarRigidBody.angularDrag = 300f;
            ProjectileSimple EngiMortarProjectileSimple = projectilePrefab.GetComponent<ProjectileSimple>();
            EngiMortarProjectileSimple.desiredForwardSpeed = 25f;
            ProjectileImpactExplosion EngiMortarImpactExplosion = projectilePrefab.GetComponent <ProjectileImpactExplosion>();
            EngiMortarImpactExplosion.falloffModel = BlastAttack.FalloffModel.None;
            EngiMortarImpactExplosion.lifetimeAfterImpact = 0;
            EngiMortarImpactExplosion.blastRadius = 5f;
            EngiMortarImpactExplosion.explosionEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiGrenadeExplosion.prefab").WaitForCompletion();
            ProjectileDamage EngiMortarProjectileDamage = projectilePrefab.GetComponent<ProjectileDamage>();
            EngiMortarProjectileDamage.damageType = DamageType.Stun1s;
            GameObject EngiMortar = Assets.mainAssetBundle.LoadAsset<GameObject>("EngiMortar"); ;
            EngiMortar.AddComponent<ProjectileGhostController>();
            //Better to set this in Unity than in-code.
            //Make sure your projectile has no colliders in Unity as well.
            EngiMortar.layer = LayerIndex.noCollision.intVal;
            ProjectileController EngiMortarProjectileController = projectilePrefab.GetComponent<ProjectileController>();
            EngiMortarProjectileController.ghostPrefab = EngiMortar;
            EngiMortar.transform.Rotate(0, 0, 1f);

        }


        protected override void RegisterStates()
        {
            ContentAddition.AddEntityState(typeof(EngiMortarFire), out bool wasAdded);
        }
    }
}