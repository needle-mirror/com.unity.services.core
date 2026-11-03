using System;
using Unity.Services.Core.Configuration.Editor;
using Unity.Services.Core.Editor.Environments.Save;
using Unity.Services.Core.Editor.Releases.Save;
using Unity.Services.Core.Editor.Settings;
using Unity.Services.Core.Environments;
using Unity.Services.Core.Releases;
using UnityEngine;
#if UNITY_6000_4_OR_NEWER
using UnityEditor;
using UnityEditor.Build.Profile;
#endif

namespace Unity.Services.Core.Editor.Releases
{
    class ReleaseConfigProvider : IConfigurationProvider
    {
        readonly IReleaseSaveSystem m_ReleaseSaveSystem;
        readonly IEnvironmentSaveSystem m_EnvironmentSaveSystem;

        // Run after EnvironmentConfigProvider (callbackOrder 0) so the build-profile
        // environment override wins when a profile with a configured environment is active.
        public int callbackOrder => 1;

        public ReleaseConfigProvider() : this(new FileSystem()) {}

        public ReleaseConfigProvider(IFileSystem fileSystem)
        {
            m_ReleaseSaveSystem = new ReleaseSaveSystem(fileSystem);
            m_EnvironmentSaveSystem = new EnvironmentSaveSystem(fileSystem);
        }

        internal ReleaseConfigProvider(IReleaseSaveSystem releaseSaveSystem, IEnvironmentSaveSystem environmentSaveSystem)
        {
            m_ReleaseSaveSystem = releaseSaveSystem;
            m_EnvironmentSaveSystem = environmentSaveSystem;
        }

        public void OnBuildingConfiguration(ConfigurationBuilder builder)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                // In editor play mode, inject the release name so ShouldImpersonate() in
                // AuthenticationNetworkClient knows which release to request via the gateway.
                // Do NOT inject the pointer or build-profile environment — those are
                // build-time settings that must not override the Project Settings selection.
                builder.SetString(ReleasesOptionsExtensions.ReleaseNameKey, GetReleaseName(forPlayMode: true));
                return;
            }
#endif
            builder.SetString(ReleasesOptionsExtensions.ReleasePointerKey, GetReleasePointerName());
            builder.SetString(ReleasesOptionsExtensions.ReleaseNameKey, GetReleaseName());
            var profileEnvName = GetBuildProfileEnvironmentName();
            if (!string.IsNullOrEmpty(profileEnvName))
                builder.SetString(EnvironmentsOptionsExtensions.EnvironmentNameKey, profileEnvName);
        }

        internal string GetReleasePointerName()
        {
#if UNITY_6000_4_OR_NEWER
            var settings = GetBuildProfileSettings();
            return settings?.selectedReleasePointerName ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        internal string GetReleaseName(bool forPlayMode = false)
        {
            if (!m_ReleaseSaveSystem.LoadLiveReleasesEnabled()) return string.Empty;
#if UNITY_6000_4_OR_NEWER
            // Build-profile builds select a release via pointer, not a direct release name.
            // In play mode, always use the per-environment release name regardless of active profile.
            if (!forPlayMode && GetBuildProfileSettings() != null) return string.Empty;
#endif
            var envId = GetEnvironmentId();
            if (string.IsNullOrEmpty(envId)) return string.Empty;
            var release = m_ReleaseSaveSystem.LoadRelease(envId);
            return string.IsNullOrEmpty(release.ReleaseName) ? string.Empty : release.ReleaseName;
        }

        string GetEnvironmentId()
        {
            var envId = m_EnvironmentSaveSystem.LoadEnvironment()?.EnvironmentId ?? Guid.Empty;
            return envId == Guid.Empty ? string.Empty : envId.ToString();
        }

        internal string GetBuildProfileEnvironmentName()
        {
#if UNITY_6000_4_OR_NEWER
            return GetBuildProfileSettings()?.selectedEnvironmentName ?? string.Empty;
#else
            return string.Empty;
#endif
        }

#if UNITY_6000_4_OR_NEWER
        static ReleasesBuildProfileSettings GetBuildProfileSettings()
        {
            var activeProfile = BuildProfile.GetActiveBuildProfile();
            if (activeProfile == null) return null;
            var path = AssetDatabase.GetAssetPath(activeProfile);
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (obj == null) continue;
                if (obj is ReleasesBuildProfileSettings s)
                    return s;
            }
            return null;
        }
#endif
    }
}
