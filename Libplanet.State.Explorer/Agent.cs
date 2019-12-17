using System;
using System.Threading.Tasks;
using Libplanet.Action;
using Libplanet.Blockchain;
using NetMQ;

namespace Libplanet.State.Explorer
{
    public class Agent<T>
        where T : IAction, new()
    {
        private NetMQPoller _poller;
        private NetMQQueue<>
        private void TipChangedHandler(object o, BlockChain<T>.TipChangedEventArgs e)
        {
        }

        public Agent(BlockChain<T> blockChain, string host, short port)
        {
            blockChain.TipChanged += TipChangedHandler;
        }

        public Task StartAsync()
        {
            
        }
    }
}