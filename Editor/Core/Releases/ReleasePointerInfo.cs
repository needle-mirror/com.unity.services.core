using System;
using Newtonsoft.Json;
using Unity.Services.Core.Releases.Client.Models;

namespace Unity.Services.Core.Editor.Releases
{
    /// <summary>
    /// The struct defining the ReleasePointerInfo.
    /// </summary>
    public struct ReleasePointerInfo
    {
        /// <summary>
        /// Name of the environment.
        /// </summary>
        [JsonProperty("name")]
        public readonly string Name;
        /// <summary>
        /// Guid of the release pointer.
        /// </summary>
        [JsonProperty("id")]
        public readonly Guid Id;
        /// <summary>
        /// If the environment is the default release pointer.
        /// </summary>
        [JsonProperty("isDefault")]
        public readonly bool IsDefault;
        /// <summary>
        /// Name of the release this pointer is currently pointing at, or null if unassigned.
        /// </summary>
        [JsonProperty("releaseName")]
        public readonly string ReleaseName;

        /// <summary>
        /// Creates a new ReleasePointerInfo instance
        /// </summary>
        /// <param name="name">The name of the release pointer.</param>
        /// <param name="id">The guid of the release pointer.</param>
        /// <param name="isDefault">If the release pointer is a default release pointer.</param>
        /// <param name="releaseName">The name of the release this pointer targets, or null if unassigned.</param>
        public ReleasePointerInfo(string name, Guid id, bool isDefault, string releaseName = null)
        {
            Name = name;
            Id = id;
            IsDefault = isDefault;
            ReleaseName = releaseName;
        }
    }
}
