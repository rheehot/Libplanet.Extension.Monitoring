using System.Collections.Generic;
using NetMQ;

namespace Libplanet.Extension.Monitoring.Messages
{
    internal sealed class GetBlockHash : Message
    {
        public readonly long BlockIndex;

        protected override MessageType Type => MessageType.GetBlockHash;

        protected override IEnumerable<NetMQFrame> DataFrames
        {
            get
            {
                yield return new NetMQFrame(NetworkOrderBitsConverter.GetBytes(BlockIndex));
            }
        }

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
