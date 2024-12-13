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
using SkillsReturns.SkillSetup.Engineer.Components;


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
            //A SteppedSkillDef keeps track of a step variable, allowing you to do things like multi-hit melee combos or alternating gun barrels
            //SteppedSkillDef inherits from SkillDef
            SteppedSkillDef steppedSkill = ScriptableObject.CreateInstance<SteppedSkillDef>();
            steppedSkill.skillName = SkillLangTokenName;
            steppedSkill.skillNameToken = SkillLangTokenName;
            steppedSkill.skillDescriptionToken = SkillLangTokenDesc;
            steppedSkill.keywordTokens = new string[] { };
            (steppedSkill as ScriptableObject).name = steppedSkill.skillName;

            //SkillStates for SteppedSkillDefs implement the SteppedSkillDef.IStepSetter interface
            //You can right click it to automatically generate the needed method
            //See: Commando's pistols
            steppedSkill.activationState = new SerializableEntityStateType(typeof(EngiMortarFire));
            steppedSkill.activationStateMachineName = "Weapon";
            steppedSkill.baseMaxStock = 1;
            steppedSkill.baseRechargeInterval = 0f;
            steppedSkill.beginSkillCooldownOnSkillEnd = false;
            steppedSkill.canceledFromSprinting = false;
            steppedSkill.cancelSprintingOnActivation = true;
            steppedSkill.fullRestockOnAssign = true;
            steppedSkill.interruptPriority = InterruptPriority.Skill;
            steppedSkill.isCombatSkill = true;
            steppedSkill.mustKeyPress = false;
            steppedSkill.rechargeStock = 1;
            steppedSkill.requiredStock = 1;
            steppedSkill.stockToConsume = 0;
            steppedSkill.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("MortarBarrageIcon");
            steppedSkill.stepCount = 2; //2 steps, 1 for each barrel

            skillDef = steppedSkill;    //We overwrite the SkillBase CreateSkillDef code and set the skillDef to be our new steppedSkill

            LanguageAPI.Add(SkillLangTokenName, "Mortar Barrage");
            LanguageAPI.Add(SkillLangTokenDesc, "<style=cIsDamage>Slowing.</style> Launch mortar rounds in a fixed arc for <style=cIsDamage>100% damage</style>.");
        }

        protected override void CreateAssets()
        {
            GameObject projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ToolbotGrenadeLauncherProjectile.prefab").WaitForCompletion()
            .InstantiateClone("SkillsReturnsEngiMortarProjectile", true);
            ContentAddition.AddProjectile(projectilePrefab);

            //Hacky custom component to fix the rotation issue. Probably bad.
            projectilePrefab.AddComponent<MortarPointAtForwardDirection>();

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
            EngiMortarImpactExplosion.impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiGrenadeExplosion.prefab").WaitForCompletion();
            ProjectileDamage EngiMortarProjectileDamage = projectilePrefab.GetComponent<ProjectileDamage>();
            EngiMortarProjectileDamage.damageType = DamageType.SlowOnHit;
            EngiMortarProjectileDamage.damageType.damageSource = DamageSource.Primary; 
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