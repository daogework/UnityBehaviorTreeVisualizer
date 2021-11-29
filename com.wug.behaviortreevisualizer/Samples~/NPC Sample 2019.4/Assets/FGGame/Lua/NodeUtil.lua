Selector = 'Selector'
Sequence = 'Sequence'
Inverter = 'Inverter'


function Node(type,name,children,...)
    return {
        type = type,
        name = name or type,
        children = children,
        params = {...}
    }
end

function LeafNode(type,...)
    return Node(type,nil,nil,...)
end

function SelectorNode(...)
    return Node(Selector,...)
end

function SequenceNode(...)
    return Node(Sequence,...)
end

function InverterNode(...)
    return Node(Inverter,...)
end