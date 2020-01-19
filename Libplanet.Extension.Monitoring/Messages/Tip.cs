using System.Collections.Generic;
using System.Security.Cryptography;
using NetMQ;

namespace Libplanet.Extension.Monitoring.Messages
{
    internal sealed class Tip : Message
    {
        public readonly long TipIndex;
        public readonly HashDigest<SHA256> TipHash;

        protected override MessageType Type => MessageType.Tip;

        protected override IEnumerable<NetMQFrame> DataFrames
        {
            get
            {
                yield return new NetMQFrame(NetworkOrderBitsConverter.GetBytes(TipIndex));
                yield return new NetMQFrame(TipHash.ToByteArray());
            }
        }

        public Tip(long tipIndex, HashDigest<SHA256> tipHash)
        {
            TipIndex = tipIndex;
            TipHash = tipHash;
        }

        public Tip(NetMQFrame[] frames)
        {
            TipIndex = frames[0].ConvertToInt64();
            TipHash = frames[1].ConvertToHashDigest<SHA256>();
        }
    }
}
