﻿using System.Security.Cryptography;
using NetMQ;

namespace Libplanet.Extension.Monitoring.Messages
{
    internal static class NetMQFrameExtension
    {
        public static HashDigest<T> ConvertToHashDigest<T>(
            this NetMQFrame frame)
            where T : HashAlgorithm
        {
            return new HashDigest<T>(frame.ToByteArray());
        }
    }
}
