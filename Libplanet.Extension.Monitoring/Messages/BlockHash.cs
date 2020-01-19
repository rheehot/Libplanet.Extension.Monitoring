using System.Collections.Generic;
using System.Security.Cryptography;
using NetMQ;

namespace Libplanet.Extension.Monitoring.Messages
{
    internal sealed class BlockHash : Message
    {
        public readonly HashDigest<SHA256> Hash;

        protected override MessageType Type => MessageType.BlockHash;

        protected override IEnumerable<NetMQFrame> DataFrames
        {
            get
            {
                yield return new NetMQFrame(Hash.ToByteArray());
            }
        }

        public BlockHash(HashDigest<SHA256> hash)
        {
            Hash = hash;
        }

        public BlockHash(NetMQFrame[] frames)
        {
            Hash = frames[0].ConvertToHashDigest<SHA256>();
        }
    }
}
