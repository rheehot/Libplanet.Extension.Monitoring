using System;
using System.Net;
using System.Security.Cryptography;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Blocks;
using Libplanet.Extension.Monitoring.Messages;
using Libplanet.Net;
using NetMQ;
using NetMQ.Sockets;

namespace Libplanet.Extension.Monitoring
{
    public sealed class Monitor : IDisposable
    {
        private readonly RequestSocket _requestSocket;

        public Monitor(DnsEndPoint endPoint)
        {
            var serverEndPoint = endPoint;

            var host = serverEndPoint.Host;
            var port = serverEndPoint.Port;
            _requestSocket = new RequestSocket($">tcp://{host}:{port}");
        }

        public IValue GetState(HashDigest<SHA256> blockHash, Address address)
        {
            var request = new GetState(blockHash, address);
            var reply = SendMultipartMessageWithReply<State>(request);
            return reply.Payload.Decode();
        }

        public Block<MonitorAction> GetBlock(HashDigest<SHA256> blockHash)
        {
            var request = new GetBlock(blockHash);
            var reply = SendMultipartMessageWithReply<Block>(request);
            return Block<MonitorAction>.Deserialize(reply.Payload);
        }

        public (long tipIndex, HashDigest<SHA256> tipHash) GetTip()
        {
            var request = new GetTip();
            var reply = SendMultipartMessageWithReply<Tip>(request);
            return (reply.TipIndex, reply.TipHash);
        }

        public HashDigest<SHA256> GetBlockHash(long blockIndex)
        {
            var request = new GetBlockHash(blockIndex);
            var reply = SendMultipartMessageWithReply<BlockHash>(request);
            return reply.Hash;
        }

        private T SendMultipartMessageWithReply<T>(Message request)
            where T : Message
        {
            _requestSocket.SendMultipartMessage(request.ToNetMQMessage());
            var raw = _requestSocket.ReceiveMultipartMessage();
            var reply = Message.Parse(raw);

            if (!(reply is T))
            {
                throw new InvalidMessageException(
                    $"Unexpected reply message, {reply.GetType()}");
            }
            else
            {
                return (T)reply;
            }
        }
        
        public class MonitorAction : IAction
        {
            private IValue _plainValue;

            public void LoadPlainValue(IValue plainValue)
            {
                _plainValue = plainValue;
            }

            public IAccountStateDelta Execute(IActionContext context)
            {
                return context.PreviousStates;
            }

            public void Render(IActionContext context, IAccountStateDelta nextStates)
            {
            }

            public void Unrender(IActionContext context, IAccountStateDelta nextStates)
            {
            }

            public IValue PlainValue => _plainValue;
        }

        public void Dispose()
        {
            _requestSocket?.Dispose();
        }
    }
}
