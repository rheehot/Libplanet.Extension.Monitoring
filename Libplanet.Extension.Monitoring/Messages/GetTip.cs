using System.Collections.Generic;
using System.Collections.Immutable;
using NetMQ;

namespace Libplanet.Extension.Monitoring.Messages
{
    internal sealed class GetTip : Message
    {
        protected override MessageType Type => MessageType.GetTip;
        protected override IEnumerable<NetMQFrame> DataFrames =>
            ImmutableArray<NetMQFrame>.Empty;
        
        public GetTip()
        {
        }

        public GetTip(NetMQFrame[] frames)
        {
        }
    }
}
