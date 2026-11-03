using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.Services.Core.Editor.Releases.Save;
using Unity.Services.Core.Editor.Settings;

namespace Unity.Services.Core.Editor.Releases
{
    class ReleaseProvider : IReleaseProvider
    {
        readonly IReleaseSaveSystem m_SaveSystem;

        public event PropertyChangedEventHandler PropertyChanged;

        public string EnvironmentId { get; set; }

        public ReleaseProvider(IReleaseSaveSystem saveSystem)
        {
            m_SaveSystem = saveSystem;
        }

        public ReleaseSettings ActiveRelease
        {
            get => m_SaveSystem.LoadRelease(EnvironmentId);
            set
            {
                m_SaveSystem.SaveRelease(EnvironmentId, value);
                OnPropertyChanged();
            }
        }

        public bool LiveReleasesEnabled
        {
            get => m_SaveSystem.LoadLiveReleasesEnabled();
            set
            {
                m_SaveSystem.SaveLiveReleasesEnabled(value);
                OnPropertyChanged();
            }
        }

        void OnPropertyChanged([CallerMemberName] string caller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
