
local table=table
local CS=CS
local require = require
local assert = assert
local print=print
_ENV = {}

RegisteredNodes={
    Selector = CS.WUG.BehaviorTreeVisualizer.Selector,
    Sequence = CS.WUG.BehaviorTreeVisualizer.Sequence,
    Inverter = CS.WUG.BehaviorTreeVisualizer.Inverter,
    IsNavigationActivityTypeOf = CS.WUG.BehaviorTreeVisualizer.IsNavigationActivityTypeOf,
    NavigateToDestination = CS.WUG.BehaviorTreeVisualizer.NavigateToDestination,
    Timer = CS.WUG.BehaviorTreeVisualizer.Timer,
    Idle = CS.WUG.BehaviorTreeVisualizer.Idle,
    AreItemsNearBy = CS.WUG.BehaviorTreeVisualizer.AreItemsNearBy,
}

function CreateTree(nodedata)
    print(nodedata.type)
    local type = RegisteredNodes[nodedata.type]
    local node = type(table.unpack(nodedata.params))
    node.Name = nodedata.name
    if nodedata.children then
        for i = 1, #nodedata.children do
            local n = CreateTree(nodedata.children[i])
            n.Parent = node
        end
    end
    return node
end

return _ENV