using Unity.Services.Core.Editor.Settings;

namespace Unity.Services.Core.Editor.Releases.Save
{
    interface IReleaseSaveSystem
    {
        void SaveLiveReleasesEnabled(bool enabled);
        bool LoadLiveReleasesEnabled();
        void SaveRelease(string environmentId, ReleaseSettings release);
        ReleaseSettings LoadRelease(string environmentId);
    }
}
