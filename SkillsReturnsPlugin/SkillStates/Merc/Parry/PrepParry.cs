using EntityStates;
using RoR2;
using UnityEngine;

namespace SkillsReturns.SkillStates.Merc.Parry
{
    public class PrepParry : BaseState
    {
        public static float baseStartDelay = 0.25f;

        private bool playedReadySound = false;
        private float startDelay;

        public override void OnEnter()
        {
            base.OnEnter();
            startDelay = baseStartDelay / attackSpeedStat;
            Util.PlayAttackSpeedSound("Play_SkillsReturns_Merc_Parry_Ready", base.gameObject, base.attackSpeedStat);

            //Play sheathe animation
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= startDelay)
            {
                if (!playedReadySound)
                {
                    playedReadySound = true;
                }
                if (isAuthority && inputBank && !inputBank.skill2.down && fixedAge >= startDelay)
                {
                    this.outer.SetNextState(new FireParry());
                    return;
                }
            }
        }

        public override void OnExit()
        {
            //Remember to reset animation state
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
