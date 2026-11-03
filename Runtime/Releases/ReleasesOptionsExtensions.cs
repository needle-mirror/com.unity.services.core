namespace Unity.Services.Core.Releases
{
    /// <summary>
    /// Initialization option extensions related to releases.
    /// </summary>
    public static class ReleasesOptionsExtensions
    {
        /// <summary>
        /// The initialization option key under which the release pointer is stored.
        /// </summary>
        public const string ReleasePointerKey = "com.unity.services.core.release-pointer";
        internal const string ReleaseNameKey = "com.unity.services.core.release-name";

        /// <summary>
        /// An extension to set the release pointer to use.
        /// </summary>
        /// <param name="self">The InitializationOptions object to modify</param>
        /// <param name="releasePointer">The release pointer to use</param>
        /// <returns>
        /// Return <paramref name="self"/>.
        /// Fluent interface pattern to make it easier to chain set options operations.
        /// </returns>
        public static InitializationOptions SetReleasePointer(this InitializationOptions self, string releasePointer)
        {
            self.SetOption(ReleasePointerKey, releasePointer);
            return self;
        }
    }
}
