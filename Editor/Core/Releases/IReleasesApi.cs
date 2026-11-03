using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.Services.Core.Editor.Environments;

namespace Unity.Services.Core.Editor.Releases
{
    /// <summary>
    /// Interface to access releases information.
    /// </summary>
    public interface IReleasesApi : INotifyPropertyChanged
    {
        /// <summary>
        /// The release pointers currently available. Used by the build profile inspector dropdown.
        /// </summary>
        IReadOnlyCollection<ReleasePointerInfo> ReleasePointers { get; }

        /// <summary>
        /// The name of the active release.
        /// </summary>
        string ActiveReleaseName { get; }

        /// <summary>
        /// The ID of the active release.
        /// </summary>
        string ActiveReleaseId { get; }

        /// <summary>
        /// The releases currently available.
        /// </summary>
        IReadOnlyCollection<ReleaseInfo> Releases { get; }

        /// <summary>
        /// Set the active release.
        /// </summary>
        /// <param name="releaseInfo">The <see cref="ReleaseInfo"/> to set as active.</param>
        void SetActiveRelease(ReleaseInfo releaseInfo);

        /// <summary>
        /// Whether the Live Releases feature is enabled by the user in Project Settings.
        /// </summary>
        bool LiveReleasesEnabled { get; }
    }
}
