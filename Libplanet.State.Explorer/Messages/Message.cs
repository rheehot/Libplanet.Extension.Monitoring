using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NetMQ;

namespace Libplanet.State.Explorer.Messages
{
    internal abstract class Message
    {
        internal enum MessageType : byte
        {
            /// <summary>
            /// Request to query calculated states.
            /// </summary>
            GetStates = 0x01,

            /// <summary>
            /// Request to query calculated states.
            /// </summary>
            States = 0x02
        }

        private static readonly ImmutableDictionary<MessageType, Type> TypeMapping = new Dictionary<MessageType, Type>
        {
            [MessageType.GetStates] = typeof(GetStates),
            [MessageType.State] = typeof(GetStates),
        }.ToImmutableDictionary();

        protected abstract MessageType Type { get; }

        protected abstract IEnumerable<NetMQFrame> DataFrames { get; }

        public static Message Parse(NetMQMessage raw)
        {
            if (raw.FrameCount == 0)
            {
                throw new ArgumentException("Can't parse empty NetMQMessage.");
            }

            var rawType = (MessageType)raw[0].ConvertToInt32();
            var messageType = TypeMapping[rawType];
            var message = (Message)Activator.CreateInstance(messageType, raw);

            return message;
        }
        
        public NetMQMessage ToNetMQMessage()
        {
            var message = new NetMQMessage();

            // Write body (by concrete class)
            foreach (NetMQFrame frame in DataFrames)
            {
                message.Append(frame);
            }

            return message;
        }

    }
}
