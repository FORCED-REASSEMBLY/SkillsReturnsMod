
using EntityStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using SkillsReturns.SharedHooks;
using SkillsReturns.SkillStates.Bandit2.FlashBang;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace SkillsReturns.SkillSetup.Bandit2
{
    public class Smokescreen : SkillBase<Smokescreen>
    {
        public override string SkillName => "Bandit - Flashbang";

        public override string SkillLangTokenName => "BANDIT2_UTILITY_SKILLSRETURNS_FLASHBANG_NAME";

        public override string SkillLangTokenDesc => "BANDIT2_UTILITY_SKILLSRETURNS_FLASHBANG_DESCRIPTION";

        public override SkillFamily SkillFamily => Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Bandit2/Bandit2BodyUtilityFamily.asset").WaitForCompletion();


        protected override void RegisterStates()
        {
            ContentAddition.AddEntityState(typeof(ThrowFlashbang), out bool wasAdded);
        }

        public static DamageAPI.ModdedDamageType SmokescreenDamageType;
        public static BuffDef BlindingDebuff;

        protected override void CreateAssets()
        {
            if (BlindingDebuff) return;
            SmokescreenDamageType = DamageAPI.ReserveDamageType();
            BlindingDebuff = SkillsReturns.Utilities.CreateBuffDef("SmokescreenDebuff", false, false, true, new Color(0.8039216f, 0.482352942f, 0.843137264f), Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBuffCloakIcon.tif").WaitForCompletion());
            BuildProjectile();
        }

        protected override void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += ApplySmokescreenDebuff;
            On.RoR2.HealthComponent.TakeDamage += ChangeDamageColor;
            SharedHooks.ModifyFinalDamage.ModifyFinalDamageActions += AmplifyDamage;
            RecalculateStatsAPI.GetStatCoefficients += Bandit2SmokebombDebuffModifier;
        }

        private void AmplifyDamage(ModifyFinalDamage.DamageMult damageMult, DamageInfo damageInfo, HealthComponent victim, CharacterBody victimBody)
        {
            if (victimBody.HasBuff(BlindingDebuff)) damageMult.value += 0.25f;
            
        }

        private void ApplySmokescreenDebuff(DamageReport report)
        {
            if (!report.victimBody) return;
            if (report.damageInfo.rejected) return;
            if (!report.damageInfo.HasModdedDamageType(SmokescreenDamageType)) return;

            report.victimBody.AddTimedBuff(BlindingDebuff, 3f);
        }

        private void ChangeDamageColor(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            //Always use a NetworkServer.active check when doing stuff with TakeDamage
            if (NetworkServer.active && self.body && self.body.HasBuff(BlindingDebuff) && damageInfo.damageColorIndex == DamageColorIndex.Default)
            {
                damageInfo.damageColorIndex = DamageColorIndex.Poison;
            }
            
            //ALWAYS remember to call orig so that the original function can run.
            orig(self, damageInfo);
        }

       

        private void Bandit2SmokebombDebuffModifier(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(BlindingDebuff))
            {
                args.moveSpeedReductionMultAdd += 0.8f;
                args.attackSpeedReductionMultAdd += 0.3f;
            }
        }


        private void BuildProjectile()
        {
            //These 2 variables are used during setup to control some stuff
            float smokeDuration = 7f;
            float smokeRadius = 15f;

            //Create smoke grenade
            //We clone Commando's grenade projectile for this
            #region build smoke grenade
            GameObject smokeProjectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/CommandoGrenadeProjectile").InstantiateClone("SkillsReturns_SmokeGrenade", true);
            ProjectileController smokeProjectileController = smokeProjectilePrefab.GetComponent<ProjectileController>();
            ProjectileDamage smokeProjectileDamage = smokeProjectilePrefab.GetComponent<ProjectileDamage>();
            smokeProjectileDamage.damageType = DamageType.Generic;
            smokeProjectileDamage.damageType.damageSource = DamageSource.Primary;
            ProjectileSimple simple = smokeProjectilePrefab.GetComponent<ProjectileSimple>();

            //Make the initial grenade projectile able to stun on first hit
            ProjectileImpactExplosion smokeProjectileImpact = smokeProjectilePrefab.GetComponent<ProjectileImpactExplosion>();

            //Create the projectile visual
            GameObject grenadeGhost = Assets.mainAssetBundle.LoadAsset<GameObject>("SmokeGrenade").InstantiateClone("SniperClassic_SmokeGhost", true);
            grenadeGhost.AddComponent<NetworkIdentity>();
            grenadeGhost.AddComponent<ProjectileGhostController>();

            //Set projectile model to the visual we created
            smokeProjectileController.ghostPrefab = grenadeGhost;

            smokeProjectileController.startSound = "";
            smokeProjectileController.procCoefficient = 1;

            //Make grenade nonlethal
            smokeProjectileDamage.crit = false;
            smokeProjectileDamage.damage = 0f;
            smokeProjectileDamage.damageColorIndex = DamageColorIndex.Default;
            smokeProjectileDamage.damageType = DamageType.Stun1s | DamageType.NonLethal;
            smokeProjectileDamage.force = 0;
            #endregion

            //Create smoke cloud
            //We clone the Mushrum Spore projectile and remove its damage, replacing its behavior.
            //This is clunky and we'd be better off making our own projectile from scratch, but lets ignore that for now.
            #region build smoke cloud
            GameObject smokeCloud = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/SporeGrenadeProjectileDotZone").InstantiateClone("SkillsReturns_SmokeDotZone", true);
            ProjectileController gasController = smokeCloud.GetComponent<ProjectileController>();
            ProjectileDamage smokeDamage = smokeCloud.GetComponent<ProjectileDamage>();

            //Remove the damage zone from the spore projectile that we're copying
            UnityEngine.Object.Destroy(smokeCloud.GetComponent<ProjectileDotZone>());
            gasController.procCoefficient = 0;

            //Make smoke damage nonlethal
            smokeDamage.crit = false;
            smokeDamage.damage = 0;
            smokeDamage.damageColorIndex = DamageColorIndex.WeakPoint;
            smokeDamage.damageType = DamageType.Stun1s | DamageType.NonLethal;
            smokeDamage.force = 0;

            //Buffs allies in the radius
            BuffWard buffWard = smokeCloud.AddComponent<BuffWard>();
            buffWard.radius = smokeRadius;
            buffWard.interval = 0.25f;
            buffWard.buffDef = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Common/bdCloak.asset").WaitForCompletion();
            buffWard.buffDuration = 1f;
            buffWard.floorWard = false;
            buffWard.expires = false;
            buffWard.invertTeamFilter = false;
            buffWard.expireDuration = 0;
            buffWard.animateRadius = false;
            buffWard.rangeIndicator = null;

            //Debuffs enemies in the radius
            BuffWard debuffWard = smokeCloud.AddComponent<BuffWard>();
            debuffWard.radius = smokeRadius;
            debuffWard.interval = 0.5f;
            debuffWard.buffDef = BlindingDebuff;
            debuffWard.buffDuration = 1f;
            debuffWard.floorWard = false;
            debuffWard.expires = false;
            debuffWard.invertTeamFilter = true;
            debuffWard.expireDuration = 0;
            debuffWard.animateRadius = false;
            debuffWard.rangeIndicator = null;

            //Set up gas VFX, based on the smokeDuration
            UnityEngine.Object.Destroy(smokeCloud.transform.GetChild(0).gameObject);
            GameObject gasFX = Assets.mainAssetBundle.LoadAsset<GameObject>("SmokeEffect").InstantiateClone("FX", false);
            gasFX.AddComponent<DestroyOnTimer>().duration = smokeDuration;
            gasFX.transform.parent = smokeCloud.transform;
            gasFX.transform.localPosition = Vector3.zero;

            //Make sure smoke is destroyed at the end of the duration.
            smokeCloud.AddComponent<DestroyOnTimer>().duration = smokeDuration;

            //Set up area indicator, this is done in a kinda hacky way
            var indicator = smokeCloud.AddComponent<HackyTeamAreaIndicator>();
            indicator.radius = smokeRadius;
            #endregion

            //Set up smoke projectile impact explosion
            smokeProjectileImpact.offsetForLifetimeExpiredSound = 1;
            smokeProjectileImpact.destroyOnEnemy = true;
            smokeProjectileImpact.destroyOnWorld = true;
            smokeProjectileImpact.timerAfterImpact = false;
            smokeProjectileImpact.falloffModel = BlastAttack.FalloffModel.None;
            smokeProjectileImpact.lifetime = 12f;
            smokeProjectileImpact.lifetimeRandomOffset = 0;
            smokeProjectileImpact.blastRadius = smokeRadius;
            smokeProjectileImpact.blastDamageCoefficient = 1;
            smokeProjectileImpact.blastProcCoefficient = 1;
            smokeProjectileImpact.fireChildren = true;
            smokeProjectileImpact.childrenCount = 1;
            smokeProjectileImpact.childrenProjectilePrefab = smokeCloud;   //Tells it to spawn our smoke cloud projectile when this triggers
            smokeProjectileImpact.childrenDamageCoefficient = 0;

            //Clone Bandit2's smokescreen effect and make the sound a part of the effect so it plays online
            GameObject impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2SmokeBomb.prefab").WaitForCompletion().InstantiateClone("SkillsReturns_SmokescreenImpactEffect", false);
            impactEffect.GetComponent<EffectComponent>().soundName = "Play_bandit2_shift_exit";
            ContentAddition.AddEffect(impactEffect);
            smokeProjectileImpact.impactEffect = impactEffect;

            //Register Projectiles
            ContentAddition.AddProjectile(smokeProjectilePrefab);
            ContentAddition.AddProjectile(smokeCloud);

            
            ThrowFlashbang.smokeProjectilePrefab = smokeProjectilePrefab;
        }

        protected override void CreateSkillDef()
        {
            base.CreateSkillDef();
            skillDef.activationState = new SerializableEntityStateType(typeof(ThrowFlashbang));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 15f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = false;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.PrioritySkill;
            skillDef.isCombatSkill = false;
            skillDef.mustKeyPress = true;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("FlashbangIcon");
            skillDef.keywordTokens = new string[] { "KEYWORD_STUNNING" };

            LanguageAPI.Add(SkillLangTokenName, "Flashbang");
            LanguageAPI.Add(SkillLangTokenDesc, "Toss a flash grenade, <style=cIsDamage>stunning and blinding</style> enemies. Blinded enemies have <style=cIsDamage>reduced mobility</style> and take <style=cIsDamage>25% more damage</style>.");

            
        }


        
    }



    //Hacky way to show team-based area indicator
    //Forcefully spawns one in, instead of trying to build it into the projectile
    public class HackyTeamAreaIndicator : MonoBehaviour
    {
        public GameObject indicatorPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/TeamAreaIndicator, FullSphere.prefab").WaitForCompletion();
        public float radius = 12f;
        private GameObject indicatorInstance;

        //TODO: Check if indicator shows for clients
        public void Awake()
        {
            TeamFilter tf = base.GetComponent<TeamFilter>();
            if (!tf) return;

            indicatorInstance = UnityEngine.Object.Instantiate(indicatorPrefab, base.transform);
            if (!indicatorInstance) return;

            TeamAreaIndicator indicator = indicatorInstance.GetComponent<TeamAreaIndicator>();
            if (!indicator) return;

            indicator.teamFilter = tf;
            indicator.transform.localScale = radius * Vector3.one;
        }

        

        private void OnDestroy()
        {
            if (indicatorInstance) UnityEngine.Object.Destroy(indicatorInstance);
        }
    }
}

