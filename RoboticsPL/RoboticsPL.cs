/*
    Copyright (c) 2020 tony48
    This file is part of RoboticsPL.

    RoboticsPL is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    RoboticsPL is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with RoboticsPL.  If not, see <https://www.gnu.org/licenses/>.

 */

using System;
using UnityEngine;

namespace RoboticsPL
{
    public class RoboticsPL : PartModule, IJointLockState
    {
        private bool isMoving = false;
        private Quaternion targetRotation;
        private ModuleAnimateGeneric animationModule = null;
        private AttachNode moveNode = null;

        [KSPField(isPersistant = false)]
        public string animationName;

        [KSPField(isPersistant = false)]
        public string nodeName;

        [KSPField(isPersistant = false)]
        public string eventName;

        [KSPEvent(guiName = "Move", guiActive=true, name = "Move")]
        public void Move()
        {
            foreach (Part p in this.part.children)
            {
                ConfigurableJoint partJoint = p.attachJoint.Joint;
                /*partJoint.xMotion = ConfigurableJointMotion.Free;
                partJoint.yMotion = ConfigurableJointMotion.Free;
                partJoint.zMotion = ConfigurableJointMotion.Free;
                partJoint.angularXMotion = ConfigurableJointMotion.Free;
                partJoint.angularYMotion = ConfigurableJointMotion.Free;
                partJoint.angularZMotion = ConfigurableJointMotion.Free;
                SoftJointLimitSpring partJointLinearLimitSpring = partJoint.linearLimitSpring;
                partJointLinearLimitSpring.spring = 0f;*/
                partJoint.connectedBody = null;
                part.rb.WakeUp();
                p.rb.isKinematic = true;
                p.transform.SetParent(moveNode.nodeTransform, true);
            }
            isMoving = true;
            animationModule.Toggle();
        }

        private void Start()
        {
            if (eventName != null)
            {
                Events["Move"].guiName = eventName;
            }
            animationModule = part.FindModulesImplementing<ModuleAnimateGeneric>().Find(m => m.animationName == animationName);
            foreach (BaseEvent e in animationModule.Events)
            {
                e.guiActive = false;
            }

            moveNode = part.FindAttachNode(nodeName);
            //Debug.Log("[ROBOTICSPL] animation: " + animationName + " nodeName: " + nodeName);
        }

        private void FixedUpdate()
        {
            if (!animationModule.IsMoving() && isMoving)
            {
                foreach (Part p in this.part.children)
                {
                    /*p.attachJoint.Joint.xMotion = ConfigurableJointMotion.Locked;
                    p.attachJoint.Joint.yMotion = ConfigurableJointMotion.Locked;
                    p.attachJoint.Joint.zMotion = ConfigurableJointMotion.Locked;
                    p.attachJoint.Joint.angularXMotion = ConfigurableJointMotion.Locked;
                    p.attachJoint.Joint.angularYMotion = ConfigurableJointMotion.Locked;
                    p.attachJoint.Joint.angularZMotion = ConfigurableJointMotion.Locked;
                    */
                    p.transform.SetParent(null, true);
                    //p.transform.position = moveNode.nodeTransform.position;
                    //p.attachJoint.Joint.targetPosition = p.transform.position;
                    p.attachJoint.Joint.connectedBody = this.part.rb;
                    p.attachJoint.Joint.autoConfigureConnectedAnchor = true;
                    p.rb.isKinematic = false;
                    //part.rb.WakeUp();
                }
                isMoving = false;
            }

            
        }

        private void Update()
        {
            if (animationModule.IsMoving() && Events["Move"].guiActive)
            {
                this.Events["Move"].guiActive = false;
            }
            else if (!animationModule.IsMoving() && !Events["Move"].guiActive)
            {
                Events["Move"].guiActive = true;
            }
        }

        public bool IsJointUnlocked()
        {
            return isMoving;
        }
    }
}