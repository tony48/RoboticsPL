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
    public class RoboticsPLServo : PartModule, IJointLockState
    {
        private bool isMoving = false; // true if the servo is moving
        private AttachNode moveNode = null;
        private Transform movingMeshTransform;
        private Quaternion targetRotation;
        private bool isFinished = true; // true if the joints aren't enabled
        private bool flightStateStarted;

        [KSPField(isPersistant = false)]
        public string nodeName;

        //[KSPField(isPersistant = false)]
        //public string eventName;

        [KSPField(isPersistant = false)] 
        public string movingMesh;

        //[KSPField(isPersistant = false)]
        //public bool isRotational;

        [KSPField(isPersistant = false)]
        public Vector3 axis;

        [KSPField(isPersistant = false)]
        public float speed = 4f;

        //[KSPField(isPersistant = false)] 
        //public float minAngle;

        //[KSPField(isPersistant = false)]
        //public float maxAngle;

        [UI_FloatRange(maxValue = 180f, minValue = -180f, stepIncrement = 1f, scene = UI_Scene.Flight)]
        [KSPField(isPersistant = true, guiActive = true, guiName = "Target Angle", guiActiveEditor = false)]
        public float targetAngle;

        //[KSPEvent(guiName = "Move 90", guiActive=true, name = "Move")]
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
            if (!isFinished || moveNode.attachedPart.transform.parent != movingMeshTransform) // if the joints are active, we need to remove them
            {
                Stack<Part> stack = new Stack<Part>();
                stack.Push(this.part);
                while (stack.Count > 0) // remove the joints of all the children
                {
                    Part item = stack.Pop();
                    //Debug.Log("[ROBOTICS] " + item.partInfo.title);
                    if (item == this.part)
                    {
                        Part child = moveNode.attachedPart;
                        ConfigurableJoint partJoint = child.attachJoint.Joint;
                        partJoint.connectedBody = null;
                        child.rb.isKinematic = true;
                        child.transform.SetParent(movingMeshTransform, true);
                        stack.Push(child);
                    }
                    else
                    {
                        foreach (Part child in item.children)
                        {
                            if (child.PhysicsSignificance == 1)
                                continue;
                            ConfigurableJoint partJoint = child.attachJoint.Joint;
                            AttachNode node = item.FindAttachNodeByPart(child);
                            partJoint.connectedBody = null;
                            child.rb.isKinematic = true;
                            if (item == this.part)
                            {
                                child.transform.SetParent(movingMeshTransform, true);
                            }
                            else
                            {
                                child.transform.SetParent(item.partTransform, true);
                            }
                            stack.Push(child);
                        }
                    }
                }
            }

            isMoving = true;
            targetRotation = Quaternion.Euler(targetAngle * axis);
            isFinished = false;
        }

        private void Start()
        {
            //if (eventName != null)
            //{
            //    Events["Move"].guiName = eventName;
            //}
            //animationModule = part.FindModulesImplementing<ModuleAnimateGeneric>().Find(m => m.animationName == animationName);
            //foreach (BaseEvent e in animationModule.Events)
            //{
            //    e.guiActive = false;
            //}
            
            moveNode = part.FindAttachNode(nodeName);
            movingMeshTransform = part.FindModelTransform(movingMesh);
            movingMeshTransform.localRotation = Quaternion.Euler(axis * targetAngle);
            UI_FloatRange range = (UI_FloatRange) Fields["targetAngle"].uiControlFlight;
            //range.minValue = minAngle;
            //range.maxValue = maxAngle;
        }

        private void FixedUpdate()
        {
            if (targetAngle == 180f)
                targetAngle = -180f;
            if (Quaternion.Euler(targetAngle * axis) != movingMeshTransform.localRotation &&
                Quaternion.Euler(targetAngle * axis) != targetRotation)
            {
                Move();
            }
            if (!isMoving && !isFinished)
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
                        partJoint.connectedBody = part.rb;
                        partJoint.autoConfigureConnectedAnchor = true; // auto set anchor
                        child.rb.isKinematic = false;
                        stack.Push(child);
                    }
                    else
                    {
                        foreach (Part child in item.children)
                        {
                            if (child.PhysicsSignificance == 1) // skip physics less parts
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
                //isMoving = false;
                isFinished = true;
            }
            else if (isMoving)
            {
                if (movingMeshTransform.localRotation == targetRotation)
                    isMoving = false;
                else
                    movingMeshTransform.localRotation = Quaternion.RotateTowards(movingMeshTransform.localRotation, targetRotation, speed * TimeWarp.deltaTime);
            }
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            if (!HighLogic.LoadedSceneIsFlight)
                return;
            // Save original positions when saving the ship.
            // Don't do it at the save occuring at initial scene start.
            if (flightStateStarted) 
            {
                foreach (Part cpart in this.vessel.parts)
                {
                    cpart.UpdateOrgPosAndRot(cpart.localRoot);
                }
            }
        }

        
        public override void OnStartFinished(StartState state)
        {
            base.OnStartFinished(state);

            flightStateStarted = true;
        }

        public bool IsJointUnlocked()
        {
            return isMoving;
        }
    }
}