using Newtonsoft.Json;

namespace Unity.Services.Core.Editor.Releases
{
    /// <summary>
    /// The struct defining the ReleaseInfo.
    /// </summary>
    public struct ReleaseInfo
    {
        /// <summary>
        /// Name of the release.
        /// </summary>
        [JsonProperty("name")]
        public readonly string Name;

        /// <summary>
        /// ID of the release.
        /// </summary>
        [JsonProperty("id")]
        public readonly string Id;

        /// <summary>
        /// Description of the release.
        /// </summary>
        [JsonProperty("description")]
        public readonly string Description;

        /// <summary>
        /// Creates a new ReleaseInfo instance.
        /// </summary>
        /// <param name="name">The name of the release.</param>
        /// <param name="id">The ID of the release.</param>
        /// <param name="description">The description of the release.</param>
        public ReleaseInfo(string name, string id, string description = null)
        {
            Name = name;
            Id = id;
            Description = description;
        }
    }
}
