namespace Libplanet.Extension.Monitoring.Messages
{
    public enum MessageType
    {
        // key: (,)
        GetTip,
        Tip,

        // key: (block-index,)
        GetBlockHash,
        BlockHash,

        // key: (block-hash,)
        GetBlockHeader,
        BlockHeader,
        GetTransactions,
        Transactions,
        
        // TODO: implement state-references with pager.
        // key: (address, page,)
        // GetStateReferences,
        // StateReferences,
        
        // key: (block-hash, address,)
        GetState,
        State,
    }
}
