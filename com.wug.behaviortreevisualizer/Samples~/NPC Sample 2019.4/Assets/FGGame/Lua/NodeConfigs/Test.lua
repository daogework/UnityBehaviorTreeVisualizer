require'NodeUtil'

return SelectorNode('test1',{
    SequenceNode('test3',{
        LeafNode('NavigateToDestination')
    }),
    SequenceNode('test2',{
        LeafNode('NavigateToDestination')
    }),
})