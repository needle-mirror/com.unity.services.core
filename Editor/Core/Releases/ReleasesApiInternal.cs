using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Unity.Services.Core.Editor.Releases
{
    class ReleasesApiInternal : IReleasesApi
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IReadOnlyCollection<ReleasePointerInfo> ReleasePointers => m_ReleasesService.ReleasePointers;
        public string ActiveReleaseName => m_ReleasesService.ActiveReleaseName;
        public string ActiveReleaseId => m_ReleasesService.ActiveReleaseId;
        public IReadOnlyCollection<ReleaseInfo> Releases => m_ReleasesService.Releases;
        public bool LiveReleasesEnabled => m_ReleasesService.LiveReleasesEnabled;

        readonly IReleasesService m_ReleasesService;

        internal ReleasesApiInternal(IReleasesService releasesService)
        {
            m_ReleasesService = releasesService;
            m_ReleasesService.PropertyChanged += OnReleasesServicePropertyChanged;
        }

        public void SetActiveRelease(ReleaseInfo releaseInfo)
        {
            m_ReleasesService.SetActiveRelease(releaseInfo);
        }

        void OnReleasesServicePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }
    }
}
