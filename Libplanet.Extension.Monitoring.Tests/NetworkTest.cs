using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Policies;
using Libplanet.Crypto;
using Libplanet.Store;
using NetMQ.Sockets;
using Xunit;

namespace Libplanet.Extension.Monitoring.Tests
{
    public class NetworkTest : IDisposable
    {
        private BlockChain<LevelUp> _chain;
        private IStore _store;
        private readonly PrivateKey _privateKey;
        private readonly Address _address;

        private Agent<LevelUp> _agent;
        private Monitor _monitor;

        private const ushort Port = 30000;
        
        public NetworkTest()
        {
            _store = new DefaultStore(path: null);
            _privateKey = new PrivateKey();
            _address = _privateKey.PublicKey.ToAddress();
            var genesisBlock = BlockChain<LevelUp>.MakeGenesisBlock(privateKey: _privateKey);
            _chain = new BlockChain<LevelUp>(new BlockPolicy<LevelUp>(), _store, genesisBlock);
            _agent = new Agent<LevelUp>(_chain, _store);
            _agent.RunAsync(Port);
            
            _monitor = new Monitor(new DnsEndPoint("localhost", Port));
        }

        public void Dispose()
        {
            _agent.Dispose();
            _monitor.Dispose();
        }

        [Fact]
        private async void GetTip()
        {
            const int repeat = 20;
            for (int i = 0; i < repeat; ++i)
            {
                await _chain.MineBlock(_privateKey.PublicKey.ToAddress());
                var (tipIndex, tipHash) = _monitor.GetTip();
                Assert.Equal(_chain.Tip.Hash, tipHash);
                Assert.Equal(_chain.Tip.Index, tipIndex);   
            }
        }

        [Fact]
        public async void GetState()
        {
            const int repeat = 20;
            for (int i = 1; i < repeat; ++i)
            {
                _chain.MakeTransaction(_privateKey, new[] { new LevelUp(),});
                await _chain.MineBlock(_address);
                var (_, tipHash) = _monitor.GetTip();
                var state = _monitor.GetState(
                    tipHash,
                    _address);
                Assert.Equal((Bencodex.Types.Integer)i, state);
            }
        }
        
        [Fact]
        public async void GetBlock()
        {
            const int repeat = 20;
            for (int i = 0; i < repeat; ++i)
            {
                await _chain.MineBlock(_address);
                var (_, tipHash) = _monitor.GetTip();
                var block = _monitor.GetBlock(tipHash);
                Assert.Equal(block.Serialize(), _chain[i].Serialize());
            }
        }

        [Fact]
        public async void GetBlockHash()
        {
            const int repeat = 20;
            for (int i = 0; i < repeat; ++i)
            {
                await _chain.MineBlock(_address);
            }

            for (int i = 0; i < repeat; ++i)
            {
                var blockHash = _monitor.GetBlockHash(i);
                Assert.Equal(_chain[i].Hash, blockHash);
            }
        }

        private class LevelUp : IAction
        {
            public void LoadPlainValue(IValue plainValue)
            {
            }

            public IAccountStateDelta Execute(IActionContext context)
            {
                var states = context.PreviousStates;

                if (context.Rehearsal)
                {
                    return states.SetState(context.Signer, default(Null));
                }

                var level = default(Bencodex.Types.Integer);
                if (states.TryGetState(context.Signer, out IValue value))
                {
                    level = (Bencodex.Types.Integer)value;
                }
                level += 1;
                states = states.SetState(context.Signer, level);
                return states;
            }

            public void Render(IActionContext context, IAccountStateDelta nextStates)
            {
            }

            public void Unrender(IActionContext context, IAccountStateDelta nextStates)
            {
            }

            public IValue PlainValue => default(Null);
        }
    }
}
