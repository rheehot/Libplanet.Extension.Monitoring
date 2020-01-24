using System.Collections.Generic;
using NetMQ;

namespace Libplanet.Extension.Monitoring.Messages
{
    internal sealed class Block : Message
    {
        public readonly byte[] Payload;

        protected override MessageType Type => MessageType.Block;

        protected override IEnumerable<NetMQFrame> DataFrames
        {
            get
            {
                yield return new NetMQFrame(Payload);
            }
        }

        public Block(byte[] payload)
        {
            Payload = payload;
        }

        public Block(NetMQFrame[] frames)
        {
            Payload = frames[0].ToByteArray();
        }
    }
}
