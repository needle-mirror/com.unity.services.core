using Unity.Services.Core.Internal;

namespace Unity.Services.Authentication.Internal
{
    /// <summary>
    /// Component providing the Release Context
    /// </summary>
    public interface IReleaseContext : IServiceComponent
    {

        /// <summary>
        /// Returns the currently used Unity Release Name provided by Auth if any
        /// </summary>
        string ReleaseName { get; }

        /// <summary>
        /// Returns the currently used Unity Release ID provided by Auth if any
        /// </summary>
        string ReleaseId { get; }

    }
}
