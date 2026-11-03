namespace Unity.Services.Core.Releases.Internal
{
    /// <inheritdoc />
    class Releases : IReleases
    {
        public string Pointer { get; internal set; }
        public string ReleaseName { get; internal set; }
    }
}
