using System;
using Unity.Services.Core.Editor.Environments.Save;
using Unity.Services.Core.Editor.Releases.Save;
using Unity.Services.Core.Editor.Settings;

namespace Unity.Services.Core.Editor.Releases
{
    /// <summary>
    /// Single authoritative check for whether a release is configured for the current
    /// environment, so external packages do not need to duplicate the file path, JSON
    /// schema, or serializer owned by <see cref="ReleaseSaveSystem"/>.
    /// </summary>
    public static class ReleaseConfigurationStatus
    {
        /// <summary>
        /// Returns true when Live Releases is enabled and the active environment has a
        /// release name configured.
        /// </summary>
        /// <returns>True if a release is configured for the current environment; otherwise false.</returns>
        public static bool IsReleaseConfiguredForCurrentEnvironment()
        {
            var fs = new FileSystem();
            var releaseSave = new ReleaseSaveSystem(fs);

            if (!releaseSave.LoadLiveReleasesEnabled())
                return false;

            var envId = new EnvironmentSaveSystem(fs).LoadEnvironment().EnvironmentId;
            if (envId == Guid.Empty)
                return false;

            return !string.IsNullOrEmpty(releaseSave.LoadRelease(envId.ToString()).ReleaseName);
        }
    }
}
