using System.Collections.Generic;
using System.Collections.Immutable;
using System.Security.Cryptography;
using NetMQ;

namespace Libplanet.Extension.Monitoring.Messages
{
    internal sealed class GetBlock : Message
    {
        public readonly HashDigest<SHA256> BlockHash;

        protected override MessageType Type => MessageType.GetBlock;

        protected override IEnumerable<NetMQFrame> DataFrames
        {
            get
            {
                yield return new NetMQFrame(BlockHash.ToByteArray());
            }
        }

        public GetBlock(HashDigest<SHA256> blockHash)
        {
            BlockHash = blockHash;
        }

        public GetBlock(NetMQFrame[] frames)
        {
            BlockHash = frames[0].ConvertToHashDigest<SHA256>();
        }
    }
}
