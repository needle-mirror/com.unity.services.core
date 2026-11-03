namespace Unity.Services.Core.Editor.Settings
{
    class ReleaseSettings
    {
        public string ReleaseName { get; set; }
        public string ReleaseId { get; set; }

        public ReleaseSettings()
        {
            ReleaseName = string.Empty;
            ReleaseId = string.Empty;
        }

        public ReleaseSettings(string releaseName, string releaseId)
        {
            ReleaseName = releaseName;
            ReleaseId = releaseId;
        }
    }
}
