using UnityEditor;

namespace Unity.Services.Core.Editor.Releases
{
    /// <summary>
    /// The class where constants related to releases are stored.
    /// </summary>
    static class ReleasesConstants
    {
        internal const string ServiceName = "Releases";
        static readonly string k_SettingsLocation = $"Project/Services/{ServiceName}";

        /// <summary>
        /// The location in Project Settings for Releases.
        /// </summary>
        public static readonly string SettingsLocation = k_SettingsLocation;

        /// <summary>
        /// A default pointer label to be used by the UI when no pointer is set.
        /// </summary>
        public static readonly string NoPointersAvailableLabel = "None";

        /// <summary>
        /// A default label to be used by the UI when no release is available.
        /// </summary>
        public static readonly string NoReleasesAvailableLabel = "None";
    }
}
