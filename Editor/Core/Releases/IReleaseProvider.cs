using System.ComponentModel;
using Unity.Services.Core.Editor.Settings;

namespace Unity.Services.Core.Editor.Releases
{
    interface IReleaseProvider : INotifyPropertyChanged
    {
        string EnvironmentId { get; set; }
        ReleaseSettings ActiveRelease { get; set; }
        bool LiveReleasesEnabled { get; set; }
    }
}
