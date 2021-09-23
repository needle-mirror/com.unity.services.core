using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Telemetry.Internal
{
    /// <summary>
    /// Component to get or create the proper <see cref="IMetricsSender"/> for a given package.
    /// </summary>
    interface IMetricsSenderFactory : IServiceComponent
    {
        /// <summary>
        /// Create a <see cref="IMetricsSender"/> setup with common tags for the given <paramref name="packageName"/>.
        /// </summary>
        /// <param name="packageName">
        /// The name of the package that will use the created <see cref="IMetricsSender"/> to send metric events.
        /// Example value: "com.unity.services.core"
        /// </param>
        /// <returns>
        /// Return a <see cref="IMetricsSender"/> setup with common tags for the given <paramref name="packageName"/>.
        /// </returns>
        IMetricsSender Create(string packageName);
    }
}
