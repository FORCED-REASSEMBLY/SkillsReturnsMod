using EntityStates.AffixVoid;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SkillsReturns.SkillSetup.Engineer.Components
{
    //Probably a bad way to do this. Is it even networked?
    public class MortarPointAtForwardDirection : MonoBehaviour
    {
        private Rigidbody rigidBody;

        private void Awake()
        {
            rigidBody = base.GetComponent<Rigidbody>();
            if (!NetworkServer.active || !rigidBody) Destroy(this); //Added netework check to attempt to fix online issues
        }

        private void FixedUpdate()
        {
            base.transform.forward = rigidBody.velocity.normalized;
        }
    }
}
