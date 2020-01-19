using System.Collections.Generic;
using NetMQ;

namespace Libplanet.Extension.Monitoring.Messages
{
    internal sealed class State : Message
    {
        public readonly byte[] Payload;

        protected override MessageType Type => MessageType.State;

        protected override IEnumerable<NetMQFrame> DataFrames
        {
            get
            {
                yield return new NetMQFrame(Payload);
            }
        }

        public State(byte[] payload)
        {
            Payload = payload;
        }

        public State(NetMQFrame[] frames)
        {
            Payload = frames[0].ToByteArray();
        }
    }
}
