﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace WUG.BehaviorTreeVisualizer
{
    public enum NavigationActivity
    {
        Waypoint, 
        PickupItem
    }

    public class NonPlayerCharacter : MonoBehaviour, IBehaviorTree, INodeDrawData
    {
        public bool useLua = false;
        public NavMeshAgent MyNavMesh { get; private set; }
        public NodeBase BehaviorTree { get {
                if (behaviorTree == null)
                {
                    GenerateBehaviorTree();
                }
                return behaviorTree;
            } set {
                behaviorTree = value;
            } }
        public NavigationActivity MyActivity { get; set; }

        public float step = 0.5f;

        private Coroutine m_BehaviorTreeRoutine;

        private NodeBase behaviorTree;


        private void Start()
        {
            MyNavMesh = GetComponent<NavMeshAgent>();
            MyActivity = NavigationActivity.Waypoint;
            GenerateBehaviorTree();
            
            if (m_BehaviorTreeRoutine == null && BehaviorTree != null)
            {
                m_BehaviorTreeRoutine = StartCoroutine(RunBehaviorTree());
            }
        }

        private void OnDestroy()
        {
            if (m_BehaviorTreeRoutine != null)
            {
                StopCoroutine(m_BehaviorTreeRoutine);
            }
        }

        private IEnumerator RunBehaviorTree()
        {
            while (enabled)
            {
                if (BehaviorTree == null)
                {
                    $"{this.GetType().Name} is missing Behavior Tree. Did you set the BehaviorTree property?".BTDebugLog();
                    continue;
                }

                (BehaviorTree as Node).Run();
                

                yield return new WaitForSeconds(step);
            }
        }

        private void GenerateBehaviorTree()
        {
            if (useLua)
            {
                BehaviorTree = FGGame.LuaUtil.CreateTree(this);
                return;
            }
            BehaviorTree = new Selector("Control NPC",
                                new Sequence("Pickup Item",
                                    new IsNavigationActivityTypeOf(NavigationActivity.PickupItem),
                                    new Selector("Look for or move to items",
                                        new Sequence("Look for items",
                                            new Inverter("Inverter",
                                                new AreItemsNearBy(5f)),
                                            new SetNavigationActivityTo(NavigationActivity.Waypoint)),
                                        new Sequence("Navigate to Item",
                                            new NavigateToDestination()))),
                                new Sequence("Move to Waypoint",
                                    new IsNavigationActivityTypeOf(NavigationActivity.Waypoint),
                                    new NavigateToDestination(),
                                    new Timer(2f,
                                        new Idle()),
                                    new SetNavigationActivityTo(NavigationActivity.PickupItem)));
            
        }

        public void ForceDrawingOfTree()
        {
            if (BehaviorTree == null)
            {
                $"Behavior tree is null - nothing to draw.".BTDebugLog();
            }

            BehaviorTreeGraphWindow.DrawBehaviorTree(BehaviorTree, true);
        }        
    }
}
