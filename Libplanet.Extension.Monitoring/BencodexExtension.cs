using Bencodex;
using Bencodex.Types;

namespace Libplanet.Extension.Monitoring
{
    public static class BencodexExtension
    {
        public static IValue Decode(this byte[] bytes)
        {
            var codec = new Codec();
            return codec.Decode(bytes);
        }

        public static byte[] Encode(this IValue value)
        {
            var codec = new Codec();
            return codec.Encode(value);
        }
    }
}
