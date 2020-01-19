using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Libplanet.Net;
using NetMQ;

namespace Libplanet.Extension.Monitoring.Messages
{
    internal abstract class Message
    {
        protected enum MessageType
        {
            // key: (,)
            GetTip,
            Tip,

            // key: (block-index,)
            GetBlockHash,
            BlockHash,

            // key: (block-hash,)
            GetBlock,
            Block,
            // TODO: implement messages below when BlockHeader became accessible.
            // GetBlockHeader,
            // BlockHeader,
            // GetTransactions,
            // Transactions,
        
            // TODO: implement state-references with pager.
            // key: (address, page,)
            // GetStateReferences,
            // StateReferences,
        
            // key: (block-hash, address,)
            GetState,
            State,
        }

        protected abstract MessageType Type { get; }

        protected abstract IEnumerable<NetMQFrame> DataFrames { get; }

        private static readonly ImmutableDictionary<MessageType, Type> TypeMapping =
            ImmutableDictionary<MessageType, Type>.Empty
                .Add(MessageType.GetTip, typeof(GetTip))
                .Add(MessageType.Tip, typeof(Tip))
                .Add(MessageType.GetBlock, typeof(GetBlock))
                .Add(MessageType.Block, typeof(Block))
                .Add(MessageType.GetBlockHash, typeof(GetBlockHash))
                .Add(MessageType.BlockHash, typeof(BlockHash))
                .Add(MessageType.GetState, typeof(GetState))
                .Add(MessageType.State, typeof(State));

        public static Message Parse(NetMQMessage raw)
        {
            if (raw.FrameCount == 0)
            {
                throw new ArgumentException(
                    $"Can't parse {nameof(Message)}, " +
                    $"because ${nameof(raw)} was empty.",
                    nameof(raw));
            }

            var rawType = (MessageType)raw[0].ConvertToInt32();
            if (!TypeMapping.TryGetValue(rawType, out Type type))
            {
                throw new InvalidMessageException(
                    $"Unknown message came. [{nameof(rawType)} == {rawType}]");
            }

            var bodyFrames = raw.Skip(1).ToArray();
            return (Message)Activator.CreateInstance(type, new[] {bodyFrames});
        }

        public NetMQMessage ToNetMQMessage()
        {
            var message = new NetMQMessage();
            foreach (NetMQFrame frame in DataFrames)
            {
                message.Append(frame);
            }

            message.Push((byte)Type);

            return message;
        }
    }
}
