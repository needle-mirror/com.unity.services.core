namespace Unity.Services.Core
{
    /// <summary>
    /// Common error codes.
    /// </summary>
    public static class CommonErrorCodes
    {
        /// <summary>
        /// Unable to determine what the error is.
        /// </summary>
        public const int Unknown = 0;

        /// <summary>
        /// DNS, TLS, and other transport errors that result in no valid HTTP response.
        /// </summary>
        public const int TransportError = 1;

        /// <summary>
        /// The request timed out because no response was received in the alotted time.
        /// </summary>
        public const int Timeout = 2;

        /// <summary>
        /// Service is unavailable. Typically returned when the service is overloaded.
        /// </summary>
        public const int ServiceUnavailable = 3;

        /// <summary>
        /// Request was rate limited. The client is making requests too frequently.
        /// </summary>
        public const int TooManyRequests = 50;

        /// <summary>
        /// The authentication token is malformed or invalid.
        /// </summary>
        public const int InvalidToken = 51;

        /// <summary>
        /// The authentication token is expired.
        /// </summary>
        public const int TokenExpired = 52;

        /// <summary>
        /// User does not have permission to perform the requested operation.
        /// </summary>
        public const int Forbidden = 53;
    }
}
