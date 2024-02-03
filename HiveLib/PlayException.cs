using System;
using System.Runtime.Serialization;

namespace HiveLib
{
    [Serializable]
    internal class PlayException : Exception
    {
        public PlayException()
        {
        }

        public PlayException(string message) : base(message)
        {
        }

        public PlayException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}