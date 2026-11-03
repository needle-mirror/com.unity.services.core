using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Services.Core.Editor.Settings;
using Unity.Services.Core.Internal.Serialization;

namespace Unity.Services.Core.Editor.Releases.Save
{
    class ReleaseSaveSystem : IReleaseSaveSystem
    {
        const string k_SettingsPath = "ProjectSettings/Packages/com.unity.services.core/ReleasesSettings.json";

        readonly IFileSystem m_FileSystem;
        readonly IJsonSerializer m_JsonSerializer;

        SettingsFile m_CachedSettings;
        bool m_IsDirty = true;
        System.DateTime m_CachedWriteTime;

        public ReleaseSaveSystem(IFileSystem fileSystem)
        {
            m_FileSystem = fileSystem;
            m_JsonSerializer = new NewtonsoftSerializer(
                new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                });
        }

        public void SaveLiveReleasesEnabled(bool enabled)
        {
            ReloadIfDirty();
            m_CachedSettings.LiveReleasesEnabled = enabled;
            Save();
        }

        public bool LoadLiveReleasesEnabled()
        {
            ReloadIfDirty();
            return m_CachedSettings.LiveReleasesEnabled;
        }

        public void SaveRelease(string environmentId, ReleaseSettings release)
        {
            if (string.IsNullOrEmpty(environmentId))
            {
                return;
            }

            ReloadIfDirty();
            var env = GetOrCreateEnvironmentRelease(environmentId);
            env.ReleaseName = release.ReleaseName;
            env.ReleaseId = release.ReleaseId;
            Save();
        }

        public ReleaseSettings LoadRelease(string environmentId)
        {
            if (string.IsNullOrEmpty(environmentId))
            {
                return new ReleaseSettings();
            }

            ReloadIfDirty();
            var env = GetEnvironmentRelease(environmentId);
            return env != null
                ? new ReleaseSettings(env.ReleaseName, env.ReleaseId)
                : new ReleaseSettings();
        }

        SettingsFile.EnvironmentRelease GetEnvironmentRelease(string environmentId)
        {
            m_CachedSettings.EnvironmentReleases.TryGetValue(environmentId, out var env);
            return env;
        }

        SettingsFile.EnvironmentRelease GetOrCreateEnvironmentRelease(string environmentId)
        {
            if (!m_CachedSettings.EnvironmentReleases.TryGetValue(environmentId, out var env))
            {
                env = new SettingsFile.EnvironmentRelease();
                m_CachedSettings.EnvironmentReleases[environmentId] = env;
            }
            return env;
        }

        void Save()
        {
            m_FileSystem.SaveFile(k_SettingsPath, m_JsonSerializer.SerializeObject(m_CachedSettings));
            m_CachedWriteTime = GetSettingsFileWriteTime();
        }

        System.DateTime GetSettingsFileWriteTime()
        {
            return m_FileSystem.GetLastWriteTime(k_SettingsPath);
        }

        void ReloadIfDirty()
        {
            var currentWriteTime = GetSettingsFileWriteTime();

            if (!m_IsDirty && currentWriteTime != m_CachedWriteTime)
            {
                m_IsDirty = true;
            }

            if (!m_IsDirty)
            {
                return;
            }

            m_CachedWriteTime = currentWriteTime;
            var fileContent = m_FileSystem.GetFileContent(k_SettingsPath);
            try
            {
                m_CachedSettings = !string.IsNullOrEmpty(fileContent)
                    ? m_JsonSerializer.DeserializeObject<SettingsFile>(fileContent) ?? new SettingsFile()
                    : new SettingsFile();
            }
            catch (JsonException)
            {
                m_CachedSettings = new SettingsFile();
            }
            m_IsDirty = false;
        }

        class SettingsFile
        {
            public bool LiveReleasesEnabled { get; set; } = false;
            public Dictionary<string, EnvironmentRelease> EnvironmentReleases { get; set; } = new Dictionary<string, EnvironmentRelease>();

            public class EnvironmentRelease
            {
                public string ReleaseName { get; set; } = string.Empty;
                public string ReleaseId { get; set; } = string.Empty;
            }
        }
    }
}
