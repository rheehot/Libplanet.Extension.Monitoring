using System.Collections.Generic;
using System.Collections.Immutable;
using NetMQ;

namespace Libplanet.Extension.Monitoring.Messages
{
    internal sealed class GetBlockHash : Message
    {
        public readonly long BlockIndex;

        protected override MessageType Type => MessageType.GetBlockHash;

        protected override IEnumerable<NetMQFrame> DataFrames
            => ImmutableArray<NetMQFrame>.Empty;

        public GetBlockHash(long blockIndex)
        {
            BlockIndex = blockIndex;
        }

        public GetBlockHash(NetMQFrame[] frames)
        {
            BlockIndex = frames[0].ConvertToInt64();
        }
    }
}
