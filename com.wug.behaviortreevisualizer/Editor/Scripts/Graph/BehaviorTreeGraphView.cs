﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace WUG.BehaviorTreeVisualizer
{
    public class BehaviorTreeGraphView : GraphView
    {
        public readonly Vector2 c_NodeSize = new Vector2(100, 150);
        private Color m_White = new Color(255, 255, 255);

        private List<Edge> m_Edges => edges.ToList();
        private List<BTGNodeData> m_Nodes => nodes.ToList().OfType<BTGNodeData>().Cast<BTGNodeData>().ToList();
        private List<BTGStackNodeData> m_StackNodes => nodes.ToList().OfType<BTGStackNodeData>().Cast<BTGStackNodeData>().ToList();

        public struct NodePositionInfo
        {
            public int TotalChildren;
            public int ChildIndex;
            public int ColumnIndex;
            public Vector2 LastNodePosition;
            public Port connectionPort;
        }

        public class FullNodeInfo
        {
            public NodeBase RunTimeNode;
            public NodeProperty PropertyData;
        }

        public BehaviorTreeGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(BehaviorTreeGraphWindow.c_StylePath));


        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //TODO: This will need to be a toggle once creating behavior trees is supported
            //      It's not needed for viewnig and can cause negative behavior
            //base.BuildContextualMenu(evt);

            if (evt.target is BTGNodeData)
            {
                BTGNodeData nodeData = (evt.target as BTGNodeData);

                int countOfItems = 1;

                if (nodeData.DecoratorData != null)
                {
                    for (int i = 0; i < nodeData.DecoratorData.Count; i++)
                    {
                        var name = nodeData.DecoratorData[0].RunTimeNode.GetType().Name;

                        evt.menu.InsertAction(0, $"Open {name}", (e) => { OpenFile($"{name}.cs"); });

                        countOfItems++;
                    }
                }

                string nodeName = nodeData.MainNodeDetails.RunTimeNode.GetType().Name;
                evt.menu.InsertAction(0, $"Open {nodeName}", (e) => { OpenFile($"{nodeName}.cs"); });

                evt.menu.InsertSeparator("", countOfItems);
            }
        }

        /// <summary>
        /// Open a file at a specified location
        /// </summary>
        /// <param name="className">Class name with extension</param>
        private void OpenFile(string className)
        {

            string[] res = Directory.GetFiles(Application.dataPath, className, SearchOption.AllDirectories);

            if (res.Length == 0)
            {
                $"Unable to locate script path. Please file a bug at https://github.com/Yecats/UnityBehaviorTreeVisualizer".BTDebugLog();
                return;
            }

            string path = res[0].Replace("\\", "/");

            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(path, 1, 0);
        }

        public void ClearTree()
        {
            m_Nodes.ForEach(x => x.parent.Remove(x));
            m_StackNodes.ForEach(x => x.parent.Remove(x));
            m_Edges.ForEach(x => x.parent.Remove(x));
        }

        public void LoadBehaviorTree(NodeBase behaviorTree)
        {
            if (behaviorTree != null)
            {
                DrawNodes(true, behaviorTree, 0, null, null);
                CalculateStackPositions();
            }
        }

        private Image CreateImage(Sprite imageIcon)
        {



            Image icon = new Image()
            {
                style =
                {
                    width = 25,
                    height = 25,
                    marginRight = 5,
                    marginTop = 5,
                    marginLeft = 5,
                }
            };

            icon.tintColor = m_White;

            if (imageIcon != null)
            {
                icon.image = imageIcon.texture;
            }

            return icon;
        }

        public void DrawNodes(bool entryPoint, NodeBase currentNode, int columnIndex, Port parentPort, StackNode stackNode, string[] styleClasses = null, List<FullNodeInfo> decoratorData = null)
        {
            int colIndex = columnIndex;

            FullNodeInfo fullDetails = new FullNodeInfo();
            fullDetails.RunTimeNode = currentNode;

            //Loses reference for some reason
            if (BehaviorTreeGraphWindow.SettingsData == null)
            {
                BehaviorTreeGraphWindow.SettingsData = new DataManager();
            }

            fullDetails.PropertyData = BehaviorTreeGraphWindow.SettingsData.GetNodeStyleDetails(currentNode);


            if (fullDetails.PropertyData != null && fullDetails.PropertyData.IsDecorator)
            {

                if (decoratorData == null)
                {
                    decoratorData = new List<FullNodeInfo>();
                }

                decoratorData.Add(fullDetails);

                if (currentNode.children.Count == 0)
                {
                    $"Decorator ({currentNode.GetType().Name}) does not have any children. Nothing will be drawn.".BTDebugLog();
                }
                else
                {
                    DrawNodes(false, currentNode.children[0], colIndex, parentPort, stackNode, null, decoratorData);
                }
            }
            else
            {

                BTGNodeData node = new BTGNodeData(fullDetails, entryPoint, parentPort, decoratorData);

                //Add general action image to title bar
                Image nodeIcon = CreateImage(fullDetails.PropertyData.Icon);
                node.titleContainer.Add(nodeIcon);
                nodeIcon.SendToBack();

                //Style the title label
                VisualElement titleLabel = node.Q<VisualElement>("title-label");
                titleLabel.style.color = new StyleColor(m_White);
                titleLabel.style.flexGrow = 1;
                titleLabel.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);

                if (!entryPoint)
                {
                    node.AddPort(GeneratePort(node, Direction.Input, Port.Capacity.Multi), "Parent", true);
                    node.GenerateEdge();

                    if (stackNode != null)
                    {
                        stackNode.AddElement(node);
                        ((BTGStackNodeData)stackNode).childNodes.Add(node);
                    }
                    else
                    {
                        AddElement(node);
                    }

                    if (styleClasses != null)
                    {
                        foreach (string style in styleClasses)
                        {
                            node.AddToClassList(style);
                        }
                    }

                }
                else
                {
                    AddElement(node);
                }

                if (currentNode.children.Count > 0)
                {
                    colIndex++;

                    BTGStackNodeData stack = m_StackNodes.FirstOrDefault(x => x.ColumnId == colIndex);

                    if (stack == null)
                    {
                        stack = new BTGStackNodeData()
                        {
                            ColumnId = colIndex,
                            style = {
                                width = 350
                            }

                        };

                        Vector2 pos = (Vector2.right * 300) * colIndex;
                        stack.SetPosition(new Rect(pos, c_NodeSize));

                        stack.RemoveFromClassList("stack-node");
                        AddElement(stack);
                    }

                    for (int i = 0; i < currentNode.children.Count; i++)
                    {
                        node.AddPort(GeneratePort(node, Direction.Output, Port.Capacity.Multi), (i + 1).ToString(), false);

                        List<string> newStyles = new List<string>();

                        if (i == 0)
                        {
                            newStyles.Add("FirstNodeSpacing");
                        }
                        else if (i == currentNode.children.Count - 1)
                        {
                            newStyles.Add("LastNodeSpacing");
                        }

                        DrawNodes(false, currentNode.children[i], colIndex, node.OutputPorts[i], stack, newStyles.ToArray());
                    }

                }
            }
        }

        private async void CalculateStackPositions()
        {
            await Task.Delay(50);

            Vector2 lastNodePosition = Vector2.zero;
            float previousStackNodeHeight = 0;

            for (int i = 0; i < m_StackNodes.Count; i++)
            {
                Rect originalInfo = m_StackNodes[i].GetPosition();

                if (i == 0)
                {
                    Rect sizeInfo = m_Nodes.FirstOrDefault(x => x.EntryPoint).GetPosition();

                    lastNodePosition = sizeInfo.center + (Vector2.down * originalInfo.height / 2) + (Vector2.right * (originalInfo.width + 75));
                    m_StackNodes[i].SetPosition(new Rect(lastNodePosition, originalInfo.size));

                    previousStackNodeHeight = sizeInfo.height;
                }
                else
                {

                    //Compare node size to previous one
                    float sizeDifference = previousStackNodeHeight - m_StackNodes[i].GetPosition().height;

                    if (sizeDifference > 100 || sizeDifference < -100)
                    {
                        BTGNodeData[] nodes = m_StackNodes[i].childNodes.FindAll(x => x.ClassListContains("FirstNodeSpacing") || x.ClassListContains("LastNodeSpacing")).ToArray();

                        foreach (BTGNodeData nodeData in nodes)
                        {

                            if (nodeData.ClassListContains("FirstNodeSpacing"))
                            {
                                nodeData.style.paddingTop = 25;
                            }
                            else
                            {
                                nodeData.style.paddingBottom = 25;
                            }
                        }

                        originalInfo = m_StackNodes[i].GetPosition();

                    }

                    Rect sizeInfo = m_StackNodes[i - 1].GetPosition();
                    Vector2 center = lastNodePosition + (Vector2.up * sizeInfo.height / 2);

                    lastNodePosition = center + (Vector2.down * originalInfo.height / 2) + (Vector2.right * (originalInfo.width + 125));

                    m_StackNodes[i].SetPosition(new Rect(lastNodePosition, originalInfo.size));

                    previousStackNodeHeight = sizeInfo.height;

                }
            }
        }

        /// <summary>
        /// Add a new port to an existing graph node
        /// </summary>
        /// <param name="targetNode">Node to add the port to</param>
        /// <param name="portDirection">Whether the port is an input or output</param>
        /// <param name="capacity">How many connections this port supports</param>
        /// <returns></returns>
        private Port GeneratePort(BTGNodeData targetNode, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return targetNode.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(bool));
        }


    }
}
