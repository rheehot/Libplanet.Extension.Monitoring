using System.Collections.Generic;
using NetMQ;

namespace Libplanet.Extension.Monitoring.Messages
{
    internal abstract class Message
    {
        protected enum MessageType
        {
            // key: (,)
            GetTip,
            Tip,

            // key: (block-index,)
            GetBlockHash,
            BlockHash,

            // key: (block-hash,)
            GetBlock,
            Block,
            // TODO: implement messages below when BlockHeader became accessible.
            // GetBlockHeader,
            // BlockHeader,
            // GetTransactions,
            // Transactions,
        
            // TODO: implement state-references with pager.
            // key: (address, page,)
            // GetStateReferences,
            // StateReferences,
        
            // key: (block-hash, address,)
            GetState,
            State,
        }

        protected abstract MessageType Type { get; }

        protected abstract IEnumerable<NetMQFrame> DataFrames { get; }

        public NetMQMessage ToNetMQMessage()
        {
            var message = new NetMQMessage();
            foreach (NetMQFrame frame in DataFrames)
            {
                message.Append(frame);
            }

            message.Push((byte)Type);

            return message;
        }
    }
}
