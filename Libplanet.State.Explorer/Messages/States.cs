using System.Collections.Generic;
using Bencodex.Types;
using NetMQ;

namespace Libplanet.State.Explorer.Messages
{
    internal class States : Message
    {
        protected override MessageType Type => MessageType.States;

        protected override IEnumerable<NetMQFrame> DataFrames { get; }

        public IValue[] States { get; }
    }
}