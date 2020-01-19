using System.Threading.Tasks;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Policies;
using Libplanet.Crypto;
using Libplanet.Store;
using NetMQ;
using NetMQ.Sockets;
using Xunit;

namespace Libplanet.Extension.Monitoring.Tests
{
    public class AgentTest
    {
        private BlockChain<LevelUp> _chain;
        private IStore _store;
        private PrivateKey _privateKey;

        private Agent<LevelUp> _agent;

        private const ushort Port = 30000;
        
        public AgentTest()
        {
            _store = new DefaultStore(path: null);
            _privateKey = new PrivateKey();
            var genesisBlock = BlockChain<LevelUp>.MakeGenesisBlock(privateKey: _privateKey);
            _chain = new BlockChain<LevelUp>(new BlockPolicy<LevelUp>(), _store, genesisBlock);
            _agent = new Agent<LevelUp>(_chain, _store);
            _agent.StartAsync(Port);
        }
        [Fact]
        public async Task Publish()
        {
            var subscriber = new SubscriberSocket();
            subscriber.Connect("tcp://localhost:30000");

            _chain.MakeTransaction(_privateKey, new[] {new LevelUp(),});
            await _chain.MineBlock(_privateKey.PublicKey.ToAddress());
            var address = subscriber.ReceiveFrameBytes();

            Assert.Equal(_privateKey.PublicKey.ToAddress().ToByteArray(), address);
        }

        ~AgentTest()
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
