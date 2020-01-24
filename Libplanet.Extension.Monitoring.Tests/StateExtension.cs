using Bencodex.Types;
using Libplanet.Action;

namespace Libplanet.Extension.Monitoring.Tests
{
    public static class StateExtension
    {
        public static bool TryGetState(this IAccountStateDelta states, Address address, out IValue value)
        {
            value = states.GetState(address);
            return value != null;
        }
    }
}