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
    public class NetworkTest
    {
        private BlockChain<LevelUp> _chain;
        private IStore _store;
        private PrivateKey _privateKey;

        private Agent<LevelUp> _agent;

        private const ushort Port = 30000;
        
        public NetworkTest()
        {
            _store = new DefaultStore(path: null);
            _privateKey = new PrivateKey();
            var genesisBlock = BlockChain<LevelUp>.MakeGenesisBlock(privateKey: _privateKey);
            _chain = new BlockChain<LevelUp>(new BlockPolicy<LevelUp>(), _store, genesisBlock);
            _agent = new Agent<LevelUp>(_chain, _store);
            _agent.RunAsync(Port);
        }

        [Fact]
        public async Task GetState()
        {
        }

        ~NetworkTest()
        {
            _agent.Dispose();
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

                if (states.TryGetState(context.Signer, out IValue value))
                {
                    var level = (Bencodex.Types.Integer)value;
                    level += 1;
                    states = states.SetState(context.Signer, level);
                }
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
