using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WUG.BehaviorTreeVisualizer
{
    public class SetNavigationActivityTo : Node
    {

        private NavigationActivity m_NewActivity;

        IEnumerator crun;

        public SetNavigationActivityTo(NavigationActivity newActivity)
        {
            m_NewActivity = newActivity;
            Name = $"Set NavigationActivity to {m_NewActivity}";
            crun = cRun();
        }
        protected override void OnReset() { crun = cRun(); }

        protected override NodeStatus OnRun()
        {
            crun.MoveNext();
            if (crun.Current is NodeStatus)
            {
                return (NodeStatus)crun.Current;
            }
            return NodeStatus.Running;
        }

        IEnumerator cRun()
        {
            if(DebugShowStep)
                yield return new WaitForSeconds(1);

            if (GameManager.Instance == null || GameManager.Instance.NPC == null)
            {
                StatusReason = "GameManager or NPC is null";
                yield return NodeStatus.Failure;
            }

            GameManager.Instance.NPC.MyActivity = m_NewActivity;

            yield return NodeStatus.Success;

        }
    }
}
