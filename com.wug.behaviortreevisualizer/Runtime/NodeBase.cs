using System.Collections.Generic;

namespace WUG.BehaviorTreeVisualizer
{

    public delegate void NodeStatusChangedEventHandler(NodeBase sender);
    public enum NodeStatus
    {
        Failure,
        Success,
        Running, //evaluation incomplete
        Unknown,
        NotRun
    }
    public class NodeBase
    {
        public string Name { get; set; }
        public string StatusReason { get; set; } = "";
        public List<NodeBase> children = new List<NodeBase>();
        public NodeBase Parent { get => parent; set {
                if (value != null)
                {
                    value.children.Add(this);
                }else if (parent != null)
                {
                    parent.children.Remove(this);
                }
                parent = value;
            } }


        public NodeStatus LastNodeStatus = NodeStatus.NotRun;
        
        public event NodeStatusChangedEventHandler NodeStatusChanged;


        protected NodeBase parent = null;

        /// <summary>
        /// Handles invoking the NodeStatusChangedEventHandler delegate.
        /// </summary>
        protected virtual void OnNodeStatusChanged(NodeBase sender)
        {
            NodeStatusChanged?.Invoke(sender);
        }

    }
}