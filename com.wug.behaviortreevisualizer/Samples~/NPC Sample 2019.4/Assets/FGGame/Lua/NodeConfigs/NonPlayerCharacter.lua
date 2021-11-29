require'NodeUtil'

return SelectorNode('Control NPC',{
    SequenceNode('Pickup Item',{
        LeafNode('IsNavigationActivityTypeOf',CS.WUG.BehaviorTreeVisualizer.NavigationActivity.PickupItem),
        SelectorNode('Look for or move to items',{
            SequenceNode("Look for items",{
                InverterNode('Inverter',{
                    LeafNode('AreItemsNearBy',5)
                }),
                LeafNode('SetNavigationActivityTo',CS.WUG.BehaviorTreeVisualizer.NavigationActivity.Waypoint),
            }),
            SequenceNode('Navigate to Item',{
                LeafNode('NavigateToDestination')
            }),
        }),
    }),
    SequenceNode('Move to Waypoint',{
        LeafNode('IsNavigationActivityTypeOf',CS.WUG.BehaviorTreeVisualizer.NavigationActivity.Waypoint),
        LeafNode('NavigateToDestination'),
        Node('Timer',nil,{
            LeafNode('Idle')
        },2.0),
        LeafNode('SetNavigationActivityTo',CS.WUG.BehaviorTreeVisualizer.NavigationActivity.PickupItem),
    }),
})
