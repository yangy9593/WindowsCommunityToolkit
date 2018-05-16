using System;

namespace LottieData.Serialization
{
    /// <summary>
    /// Exception thrown to indicate a problem reading a Lottie composition.
    /// </summary>
    sealed class LottieJsonReaderException : Exception
    {
        public LottieJsonReaderException()
        {
        }

        public LottieJsonReaderException(string message) : base(message)
        {
        }

        public LottieJsonReaderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
