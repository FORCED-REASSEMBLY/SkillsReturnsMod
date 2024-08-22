using R2API;
using RoR2;
using UnityEngine;

namespace SkillsReturns
{
    internal class Utilities
    {
        public static BuffDef CreateBuffDef(string name, bool canStack, bool isCooldown, bool isDebuff, Color color, Sprite iconSprite)
        {
            BuffDef bd = ScriptableObject.CreateInstance<BuffDef>();
            bd.name = name;
            bd.canStack = canStack;
            bd.isCooldown = isCooldown; //Allows it to be cleansed by  blast shower
            bd.isDebuff = isDebuff;
            bd.buffColor = color;
            bd.iconSprite = iconSprite;
            ContentAddition.AddBuffDef(bd);

            (bd as ScriptableObject).name = bd.name;
            return bd;
        }
    }
}
