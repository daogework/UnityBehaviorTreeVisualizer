namespace WUG.BehaviorTreeVisualizer
{
    public abstract class Decorator : Node
    {
        public Decorator() { }
        public Decorator(string name, Node node=null)
        {
            Name = name;
            if(node !=null)
                children.Add(node);
        }

    }
}
