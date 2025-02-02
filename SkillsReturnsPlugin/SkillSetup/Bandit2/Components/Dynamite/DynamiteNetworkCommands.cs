using RoR2;
using UnityEngine.Networking;

namespace SkillsReturns.SkillSetup.Bandit2.Components.Dynamite
{
    public class DynamiteNetworkCommands : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcResetSpecialCooldown()
        {
            if (this.hasAuthority)
            {
                skillLocator.special.stock = skillLocator.special.maxStock;
                skillLocator.special.rechargeStopwatch = 0f;
            }
        }

        private void Awake()
        {
            characterBody = base.GetComponent<CharacterBody>();
            skillLocator = base.GetComponent<SkillLocator>();
        }

        private SkillLocator skillLocator;
        private CharacterBody characterBody;
    }
}
