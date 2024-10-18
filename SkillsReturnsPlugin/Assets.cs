using R2API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SkillsReturns
{
    internal static class Assets
    {
        public static AssetBundle mainAssetBundle;

        internal static void Init()
        {
            using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SkillsReturns.skillsreturnsbundle"))
            {
                mainAssetBundle = AssetBundle.LoadFromStream(manifestResourceStream);

                //Async method seems to have issues, so I'm using this.
                ShaderSwapper.ShaderSwapper.UpgradeStubbedShaders(mainAssetBundle);
            }

            using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SkillsReturns.SkillsReturnsSoundbank.bnk"))
            {

                byte[] array = new byte[manifestResourceStream.Length];
                manifestResourceStream.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }
        }
    }
}
