using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
namespace SkillsReturns.SharedHooks
{
    internal static class ModifyFinalDamage
    {
        public delegate void ModifyFinalDamageDelegate(DamageMult damageMult, DamageInfo damageInfo,
            HealthComponent victim, CharacterBody victimBody);
        public static ModifyFinalDamageDelegate ModifyFinalDamageActions;

        internal static void ModfyFinalDamageHook(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(
                 x => x.MatchStloc(7)
                ))
            {
                c.Emit(OpCodes.Ldarg_0);    //self
                c.Emit(OpCodes.Ldarg_1);    //damageInfo
                c.EmitDelegate<Func<float, HealthComponent, DamageInfo, float>>((origDamage, victimHealth, damageInfo) =>
                {
                    float newDamage = origDamage;
                    CharacterBody victimBody = victimHealth.body;
                    if (victimBody)
                    {
                        DamageMult damageMult = new DamageMult();
                        ModifyFinalDamageActions?.Invoke(damageMult, damageInfo, victimHealth, victimBody);
                        newDamage *= damageMult.value;
                    }
                    return newDamage;
                });
            }
            else
            {
                UnityEngine.Debug.LogError("SkillsReturns: ModifyFinalDamage IL Hook failed. This will break a lot of things.");
            }
        }

        //This is used so that the damage multiplier persists
        internal class DamageMult
        {
            public float value = 1f;
        }
    }
}
