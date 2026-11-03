using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Unity.Services.Core.Editor.Releases
{
    interface IReleasesService : INotifyPropertyChanged
    {
        // Release Pointer list (read-only; used by build profile inspector dropdown)
        IReadOnlyCollection<ReleasePointerInfo> ReleasePointers { get; }
        Task RefreshReleasePointersAsync();

        /// <summary>
        /// One-shot fetch for a specific environment — does not update global state or ReleasePointers.
        /// </summary>
        Task<IReadOnlyList<ReleasePointerInfo>> FetchReleasePointersForEnvironmentAsync(string environmentId);

        // Release
        IReadOnlyCollection<ReleaseInfo> Releases { get; }
        string ActiveReleaseName { get; }
        string ActiveReleaseId { get; }
        void SetActiveRelease(ReleaseInfo releaseInfo);
        void ClearActiveRelease();
        Task RefreshReleasesAsync();

        // Shared
        bool LiveReleasesEnabled { get; }
        void SetLiveReleasesEnabled(bool enabled);
    }
}
