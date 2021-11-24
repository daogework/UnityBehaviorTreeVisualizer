namespace WUG.BehaviorTreeVisualizer
{
    public class Selector : Composite
    {
        int currentChild = 0;

        /// <summary>
        /// Runs each child until one of them returns status Success. Will return failure if no child runs successfully.
        /// </summary>
        /// <param name="name">Friendly name - displayed on the Behavior Tool Debugger UI if set</param>
        /// <param name="childNodes">Children node(s) to run</param>
        public Selector(string name, params Node[] childNodes) : base(name, childNodes) { }
        public Selector() { }
        protected override void OnReset() 
        { 
            currentChild = 0;

            for (int i = 0; i < children.Count; i++)
            {
                (children[i] as Node).Reset();
            }
        }

        protected override NodeStatus OnRun()
        {
            if (currentChild >= children.Count)
            {
                return NodeStatus.Failure;
            }

            NodeStatus nodeStatus = (children[currentChild]as Node).Run();

            switch (nodeStatus)
            {
                case NodeStatus.Failure:
                    currentChild++;
                    break;
                case NodeStatus.Success:
                    return NodeStatus.Success;
            }

            //in progress
            return NodeStatus.Running;
        }

    }
}
