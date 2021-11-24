require'NodeUtil'

return Node(Selector,'Control NPC',{
    Node(Sequence,'Pickup Item',{
        LeafNode('IsNavigationActivityTypeOf',CS.WUG.BehaviorTreeVisualizer.NavigationActivity.PickupItem),
        Node(Selector,'Look for or move to items',{
            Node(Sequence,"Look for items",{
                Node(Inverter,'Inverter',{
                    LeafNode('AreItemsNearBy',0.5)
                }),
                LeafNode('IsNavigationActivityTypeOf',CS.WUG.BehaviorTreeVisualizer.NavigationActivity.Waypoint),
            }),
            Node(Sequence,'Navigate to Item',{
                LeafNode('NavigateToDestination')
            }),
        }),
    }),
    Node(Sequence,'Move to Waypoint',{
        LeafNode('IsNavigationActivityTypeOf',CS.WUG.BehaviorTreeVisualizer.NavigationActivity.Waypoint),
        LeafNode('NavigateToDestination'),
        Node('Timer',nil,{
            LeafNode('Idle')
        },2.0),
        LeafNode('IsNavigationActivityTypeOf',CS.WUG.BehaviorTreeVisualizer.NavigationActivity.PickupItem),
    }),
})
