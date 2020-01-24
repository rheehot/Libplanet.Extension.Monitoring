using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Policies;
using Libplanet.Crypto;
using Libplanet.Store;
using Xunit;

namespace Libplanet.Extension.Monitoring.Tests
{
    public sealed class NetworkTest : IDisposable
    {
        private readonly BlockChain<LevelUp> _chain;
        [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
        private readonly IStore _store;
        private readonly PrivateKey _privateKey;
        private readonly Address _address;

        private readonly Agent<LevelUp> _agent;
        private readonly Monitor _monitor;

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
        private async Task GetTip()
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
        public async Task GetState()
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
        public async Task GetBlock()
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
        public async Task GetBlockHash()
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
                // There is no value to store in the chain.
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
                // Do nothing.
            }

            public void Unrender(IActionContext context, IAccountStateDelta nextStates)
            {
                // Do nothing.
            }

            public IValue PlainValue => default(Null);
        }
    }
}
