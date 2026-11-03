namespace Unity.Services.Core.Editor.Releases
{
    /// <summary>
    /// Class that grants access to the <see cref="IReleasesApi"/> instance.
    /// </summary>
    public static class ReleasesApi
    {
        /// <summary>
        /// Instance of the <see cref="IReleasesApi"/> class.
        /// </summary>
        public static IReleasesApi Instance => ReleaseServiceRegistry.Instance.GetService<IReleasesApi>();
    }
}
