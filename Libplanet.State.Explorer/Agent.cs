using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Bencodex;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Store;
using NetMQ;
using NetMQ.Monitoring;
using NetMQ.Sockets;
using Serilog;

namespace Libplanet.State.Explorer
{
    public class Agent<T>
        where T : IAction, new()
    {
        private IStore _store;
        private BlockChain<T> _chain;

        private PublisherSocket _publisher;
        private NetMQMonitor _monitor;

        private void TipChangedHandler(object o, BlockChain<T>.TipChangedEventArgs e)
        {
            var blockHash = e.Hash;
            var updatedAddresses = _chain[blockHash].Transactions
                .SelectMany(tx => tx.UpdatedAddresses)
                .ToImmutableHashSet();

            var codec = new Codec();
            foreach (var address in updatedAddresses)
            {
                var state = _chain.GetState(address, blockHash);
                _publisher
                    .SendMoreFrame(address.ToByteArray())
                    .SendMoreFrame(NetworkOrderBitsConverter.GetBytes(e.Index))
                    .SendMoreFrame(e.Hash.ToByteArray())
                    .SendMoreFrame(e.PreviousHash?.ToByteArray() ?? new byte[0])
                    .SendFrame(codec.Encode(state));
            }
        }

        public Agent(BlockChain<T> blockChain, IStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));

            _chain = blockChain ?? throw new ArgumentNullException(nameof(blockChain));
            _chain.TipChanged += TipChangedHandler;
        }

        public Task StartAsync(ushort port)
        {
            _publisher = new PublisherSocket($"tcp://*:{port}");
            _monitor = new NetMQMonitor(_publisher, "inproc://pub.inproc", SocketEvents.Accepted);
            _monitor.Accepted += (sender, args) => { Console.WriteLine(args.Address); };
            return _monitor.StartAsync();
        }

        public void Dispose()
        {
            _chain.TipChanged -= TipChangedHandler;
            _publisher?.Close();
            _monitor?.Stop();
        }
    }
}
