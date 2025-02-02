using EntityStates;
using R2API;
using RoR2.Skills;
using SkillsReturns.SkillStates.Merc.Parry;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace SkillsReturns.SkillSetup.Merc
{
    public class Parry : SkillBase<Parry>
    {
        public override string SkillName => "Mercenary - Focused Strike";

        public override string SkillLangTokenName => "MERC_SECONDARY_SKILLSRETURNS_PARRY_NAME";

        public override string SkillLangTokenDesc => "MERC_SECONDARY_SKILLSRETURNS_PARRY_DESCRIPTION";

        public override SkillFamily SkillFamily => Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Merc/MercBodySecondaryFamily.asset").WaitForCompletion();

        protected override void CreateAssets()
        {
            FireParry.soundSlashStandard = Utilities.CreateNetworkSoundEventDef("Play_SkillsReturns_Merc_Parry_StandardSlash");
            FireParry.soundSlashSuccessful = Utilities.CreateNetworkSoundEventDef("Play_SkillsReturns_Merc_Parry_SuccessfulSlash");
            FireParry.parryBuff = Utilities.CreateBuffDef("SkillsReturnsMercParry", true, false, false, Color.white, null, true);
        }
        protected override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            if (NetworkServer.active)
            {
                if (self.body.HasBuff(FireParry.parryBuff))
                {
                    self.body.AddTimedBuff(FireParry.parryBuff, 10f);   //duration is arbitrary
                    damageInfo.rejected = true;
                }
            }
            orig(self, damageInfo);
        }

        protected override void CreateSkillDef()
        {
            base.CreateSkillDef();
            skillDef.activationState = new SerializableEntityStateType(typeof(PrepParry));
            skillDef.activationStateMachineName = "Body";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 5f;
            skillDef.beginSkillCooldownOnSkillEnd = true;
            skillDef.canceledFromSprinting = false;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Skill;
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = true;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("CombatKnifeIcon");
            skillDef.keywordTokens = new string[] { "KEYWORD_STUNNING", "KEYWORD_EXPOSE" };

            LanguageAPI.Add(SkillLangTokenName, "Focused Strike");
            LanguageAPI.Add(SkillLangTokenDesc, "<style=cIsDamage>Stunning</style>. Hold to sheathe your weapon. Release before an incoming strike to <style=cIsDamage>parry</style> enemy attacks for <style=cIsDamage>500%-1500% damage</style>. Perfect parries <style=cIsUtility>Expose</style> enemies.");
        }

        protected override void RegisterStates()
        {
            bool wasAdded;
            ContentAddition.AddEntityState(typeof(PrepParry), out wasAdded);
            ContentAddition.AddEntityState(typeof(FireParry), out wasAdded);
        }
    }
}
