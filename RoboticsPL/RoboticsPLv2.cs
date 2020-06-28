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
using System.Collections.Generic;

namespace RoboticsPL
{
    public class RoboticsPLv2 : PartModule, IJointLockState
    {
        private bool isMoving = false;
        //private Quaternion targetRotation;
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
            /*foreach (Part p in this.part.children)
            {
                ConfigurableJoint partJoint = p.attachJoint.Joint;
                partJoint.connectedBody = null;
                part.rb.WakeUp();
                p.rb.isKinematic = true;
                p.transform.SetParent(moveNode.nodeTransform, true);
            }*/
            Stack<Part> stack = new Stack<Part>();
            stack.Push(this.part);
            while (stack.Count > 0)
            {
                Part item = stack.Pop();
                //Debug.Log("[ROBOTICS] " + item.partInfo.title);
                if (item == this.part)
                {
                    Part child = moveNode.attachedPart;
                    ConfigurableJoint partJoint = child.attachJoint.Joint;
                    partJoint.connectedBody = null;
                    //item.rb.WakeUp();
                    child.rb.isKinematic = true;
                    child.transform.SetParent(moveNode.nodeTransform, true);
                    stack.Push(child);
                }
                else
                {
                    foreach (Part child in item.children)
                    {
                        if (child.PhysicsSignificance == 1)
                            continue;
                        ConfigurableJoint partJoint = child.attachJoint.Joint;
                        //Debug.Log("[ROBOTICS] " + child.partInfo.title);
                        //Debug.Log("[ROBOTICS] Count " + item.attachNodes.Count);
                        /*foreach (AttachNode n in item.attachNodes)
                        {
                            if (n.attachedPart == null)
                            {
                                Debug.Log("[ROBOTICS] attached part is null");
                                continue;
                            }
    
                            Debug.Log("[ROBOTICS] " + n.attachedPart.partInfo.title);
                            Debug.Log("[ROBOTICS] " + n.attachedPartId);
                            Debug.Log("[ROBOTICS] " + n.attachedPart.flightID);
                        }*/
                        AttachNode node = item.FindAttachNodeByPart(child);
                        /*if (node == null)
                        {
                            Debug.Log("NODE IS NULL WTF KSP");
                        }*/
                        //Debug.Log("[ROBOTICS] " + node.id);
                        //Debug.Log("1");
                        partJoint.connectedBody = null;
                        //Debug.Log("2");
                        //item.rb.WakeUp();
                        //Debug.Log("3");
                        child.rb.isKinematic = true;
                        //Debug.Log("4");
                        if (item == this.part)
                        {
                            child.transform.SetParent(node.nodeTransform, true);
                        }
                        else
                        {
                            child.transform.SetParent(item.partTransform, true);
                        }

                        //Debug.Log("5");
                        stack.Push(child);
                        //Debug.Log("6");
                    }
                }
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
                /*foreach (Part p in this.part.children)
                {
                    p.transform.SetParent(null, true);
                    p.attachJoint.Joint.connectedBody = this.part.rb;
                    p.rb.isKinematic = false;
                }*/
                Stack<Part> stack = new Stack<Part>();
                stack.Push(this.part);
                while (stack.Count > 0)
                {
                    Part item = stack.Pop();
                    if (item == this.part)
                    {
                        Part child = moveNode.attachedPart;
                        ConfigurableJoint partJoint = child.attachJoint.Joint;
                        child.transform.SetParent(null, true);
                        partJoint.connectedBody = this.part.rb;
                        partJoint.autoConfigureConnectedAnchor = true;
                        child.rb.isKinematic = false;
                        //partJoint.autoConfigureConnectedAnchor = false;
                        stack.Push(child);
                    }
                    else
                    {
                        foreach (Part child in item.children)
                        {
                            if (child.PhysicsSignificance == 1)
                                continue;
                            ConfigurableJoint partJoint = child.attachJoint.Joint;
                            child.transform.SetParent(null, true);
                            partJoint.connectedBody = item.rb;
                            //partJoint.autoConfigureConnectedAnchor = true;
                            child.rb.isKinematic = false;
                            stack.Push(child);
                        }
                    }
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