using EntityStates.AffixVoid;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SkillsReturns.SkillSetup.Engineer.Components
{
    //Probably a bad way to do this. Is it even networked?
    public class MortarPointAtForwardDirection : MonoBehaviour
    {
        private Rigidbody rigidBody;

        private void Awake()
        {
            rigidBody = base.GetComponent<Rigidbody>();
            if (!rigidBody) Destroy(this);
        }

        private void FixedUpdate()
        {
            base.transform.forward = rigidBody.velocity.normalized;
        }
    }
}
