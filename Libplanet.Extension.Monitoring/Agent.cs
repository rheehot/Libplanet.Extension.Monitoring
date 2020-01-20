using System;
using Bencodex;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Extension.Monitoring.Messages;
using Libplanet.Net;
using Libplanet.Store;
using NetMQ;
using NetMQ.Sockets;
using Serilog;

namespace Libplanet.Extension.Monitoring
{
    public class Agent<T> : IDisposable
        where T : IAction, new()
    {
        private IStore _store;
        private BlockChain<T> _chain;

        private ResponseSocket _server;
        private NetMQPoller _poller;

        public Agent(BlockChain<T> blockChain, IStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _chain = blockChain ?? throw new ArgumentNullException(nameof(blockChain));
        }

        public void RunAsync(ushort port)
        {
            _server = new ResponseSocket($"tcp://*:{port}");
            _poller = new NetMQPoller { _server };

            _server.ReceiveReady += ProcessMessage;
            
            _poller.RunAsync();
        }

        public void Dispose()
        {
            _poller?.Dispose();
            _server?.Dispose();
        }

        private void ProcessMessage(object sender, NetMQSocketEventArgs eventArgs)
        {
            try
            {
                NetMQMessage raw = eventArgs.Socket.ReceiveMultipartMessage();
                NetMQSocket client = eventArgs.Socket;
                Message message = Message.Parse(raw);
                Message reply = null;
                switch (message)
                {
                    case GetBlock getBlock:
                        var block = _chain[getBlock.BlockHash];
                        var payload = block.Serialize();
                        reply = new Block(payload);
                        break;
                    case GetTip getTip:
                        var tip = _chain.Tip;
                        reply = new Tip(tip.Index, tip.Hash);
                        break;
                    case GetState getState:
                        var state = _chain.GetState(
                                        getState.Address,
                                        getState.BlockHash) ?? default(Null);
                        var codec = new Codec();
                        reply = new State(codec.Encode(state));
                        break;
                    case GetBlockHash getBlockHash:
                        var blockHash = _chain[getBlockHash.BlockIndex].Hash;
                        reply = new BlockHash(blockHash);
                        break;
                    default:
                        throw new InvalidMessageException(
                            $"Unhandled message [type: {message.GetType()}].");
                }

                client.SendMultipartMessage(reply.ToNetMQMessage());
            }
            catch (Exception e)
            {
                Log.Error("Error occurred.", e);
            }
        }
    }
}
