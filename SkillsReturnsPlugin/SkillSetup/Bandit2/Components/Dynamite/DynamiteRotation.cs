using UnityEngine.Networking;
using UnityEngine;
using RoR2.Projectile;

namespace SkillsReturns.SkillSetup.Bandit2.Components.Dynamite
{
    public class DynamiteRotation : MonoBehaviour
    {
        public void Awake()
        {
            projectileImpactExplosion = base.gameObject.GetComponent<ProjectileImpactExplosion>();
        }

        public void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                if (projectileImpactExplosion.hasImpact)
                {
                    Destroy(this);
                }
                base.transform.rotation = Quaternion.AngleAxis(-360f * Time.fixedDeltaTime, Vector3.right) * base.transform.rotation;
            }
        }

        private ProjectileImpactExplosion projectileImpactExplosion;
    }
}
