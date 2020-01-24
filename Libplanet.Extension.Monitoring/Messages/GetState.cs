using System.Collections.Generic;
using System.Security.Cryptography;
using NetMQ;

namespace Libplanet.Extension.Monitoring.Messages
{
    internal sealed class GetState : Message
    {
        public readonly HashDigest<SHA256> BlockHash;

        public readonly Address Address;

        protected override MessageType Type => MessageType.GetState;

        protected override IEnumerable<NetMQFrame> DataFrames
        {
            get
            {
                yield return new NetMQFrame(BlockHash.ToByteArray());
                yield return new NetMQFrame(Address.ToByteArray());
            }
        }

        public GetState(HashDigest<SHA256> blockHash, Address address)
        {
            BlockHash = blockHash;
            Address = address;
        }

        public GetState(NetMQFrame[] frames)
        {
            BlockHash = frames[0].ConvertToHashDigest<SHA256>();
            Address = new Address(frames[1].ToByteArray());
        }
    }
}
