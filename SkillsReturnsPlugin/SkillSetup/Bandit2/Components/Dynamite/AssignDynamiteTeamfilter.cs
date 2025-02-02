using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;

namespace SkillsReturns.SkillSetup.Bandit2.Components.Dynamite
{
    public class AssignDynamiteTeamFilter : MonoBehaviour
    {
        public bool fired = false;
        private void Start()
        {
            if (NetworkServer.active)
            {
                TeamComponent teamComponent = base.GetComponent<TeamComponent>();
                if (teamComponent) teamComponent.teamIndex = TeamIndex.None;
            }
        }
    }
}
