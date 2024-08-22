using BepInEx.Configuration;
using Facepunch.Steamworks;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SkillsReturns.SkillSetup
{
    //Based off of TILER2 Item Module
    public abstract class SkillBase<T> : SkillBase where T : SkillBase<T>
    {
        public static T Instance { get; private set; }

        public SkillBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting SkillBase was instantiated twice");
            Instance = this as T;
            Instance.Init();
        }
    }

    public abstract class SkillBase
    {
        public abstract string SkillName { get; }   //Internal Skill Name, used for Config
        public abstract string SkillLangTokenName { get; }  //Actual Language Token Name
        public abstract string SkillLangTokenDesc { get; }  //Actual Language Token Desc
        public abstract SkillFamily SkillFamily { get; }

        private bool enabled;   //This is only used on initial setup. Changing this after setup does nothing.
        public SkillDef skillDef;

        protected void Init()
        {
            CreateConfig();
            if (!enabled) return;
            CreateAssets();
            Hooks();
            RegisterStates();
            CreateSkillDef();
            AddSkillToSlot();
        }

        //Do a bit of the tedious setup here.
        protected virtual void CreateSkillDef()
        {
            skillDef = ScriptableObject.CreateInstance<SkillDef>();
            skillDef.skillName = SkillLangTokenName;
            skillDef.skillNameToken = SkillLangTokenName;
            skillDef.skillDescriptionToken = SkillLangTokenDesc;
            skillDef.keywordTokens = new string[] { };

            (skillDef as ScriptableObject).name = SkillLangTokenName;
            ContentAddition.AddSkillDef(skillDef);
        }

        protected virtual void CreateAssets() { }   //Use this to initialize skill-specific assets if needed.
        protected virtual void Hooks() { }  //Use this for your hooks
        protected virtual void CreateConfig()
        {
            enabled = SkillsReturnsPlugin.configFile.Bind<bool>(new ConfigDefinition(SkillName, "Enabled"), true, new ConfigDescription("Enable this skill.")).Value;
        }

        protected abstract void RegisterStates();

        protected virtual void AddSkillToSlot()
        {
            if (!skillDef)
            {
                Debug.LogError("SkillsReturns: Could not add " + SkillName + ": SkillDef is null.");
                return;
            }

            if (SkillFamily == null)
            {
                Debug.LogError("SkillsReturns: Could not add " + SkillName + ": SkillFamily is null.");
                return;
            }

            SkillFamily skillFamily = SkillFamily;
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }
    }
}
