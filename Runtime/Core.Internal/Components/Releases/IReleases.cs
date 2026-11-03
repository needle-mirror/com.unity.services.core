using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Releases.Internal
{
    /// <summary>
    /// Component providing the Unity Release Pointer
    /// </summary>
    public interface IReleases : IServiceComponent
    {
        /// <summary>
        /// Returns the name of the currently used Unity Release Pointer
        /// </summary>
        string Pointer { get; }

        /// <summary>
        /// Returns the name of the currently used Unity Release
        /// </summary>
        string ReleaseName { get; }
    }
}
