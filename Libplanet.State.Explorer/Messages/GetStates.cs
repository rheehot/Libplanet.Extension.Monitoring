using System.Collections.Generic;
using System.Linq;
using NetMQ;

namespace Libplanet.State.Explorer.Messages
{
    internal class GetStates : Message
    {
        protected override MessageType Type => MessageType.GetStates;

        protected override IEnumerable<NetMQFrame> DataFrames
        {
            get
            {
                foreach (Address address in Addresses)
                {
                    yield return new NetMQFrame(address.ToByteArray());
                }
            }
        }

        public Address[] Addresses { get; }

        internal GetStates(NetMQMessage raw)
        {
            Addresses = raw.Skip(1).Select(frame => new Address(frame.Buffer)).ToArray();
        }
    }
}
