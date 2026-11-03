using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Core.Editor.Environments;
using Unity.Services.Core.Editor.Settings;
using Unity.Services.Core.Editor.Shared.EditorUtils;
using Unity.Services.Core.Internal;

namespace Unity.Services.Core.Editor.Releases
{
    sealed class ReleasesService : IReleasesService, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IReadOnlyCollection<ReleasePointerInfo> ReleasePointers { get; private set; }

        public IReadOnlyCollection<ReleaseInfo> Releases { get; private set; }

        public string ActiveReleaseName =>
            m_ReleaseProvider.LiveReleasesEnabled
                ? string.IsNullOrEmpty(m_ReleaseProvider.ActiveRelease.ReleaseName) ? null : m_ReleaseProvider.ActiveRelease.ReleaseName
                : null;

        public string ActiveReleaseId =>
            m_ReleaseProvider.LiveReleasesEnabled
                ? m_ReleaseProvider.ActiveRelease.ReleaseId
                : null;

        public bool LiveReleasesEnabled => m_ReleaseProvider.LiveReleasesEnabled;

        readonly IEnvironmentService m_EnvironmentService;
        readonly IReleaseFetcher m_Fetcher;
        readonly IReleaseProvider m_ReleaseProvider;

        CancellationTokenSource m_RefreshPointersCts;
        Task m_RefreshPointersTask;
        CancellationTokenSource m_RefreshReleasesCts;
        Task m_RefreshReleasesTask;

        public ReleasesService(
            IEnvironmentService environmentService,
            IReleaseFetcher fetcher,
            IReleaseProvider releaseProvider)
        {
            m_EnvironmentService = environmentService;
            m_Fetcher = fetcher;
            m_ReleaseProvider = releaseProvider;

            m_EnvironmentService.PropertyChanged += OnEnvironmentServicePropertyChanged;

            var initialEnvId = m_EnvironmentService.ActiveEnvironmentId?.ToString() ?? string.Empty;
            m_ReleaseProvider.EnvironmentId = initialEnvId;

            if (m_EnvironmentService.ActiveEnvironmentId != null)
            {
                // Defer until the next editor update so all [InitializeOnLoadMethod] handlers
                // (including ThreadHelper.Init()) have run before any UnityWebRequest is created.
                Sync.RunNextUpdateOnMain(() =>
                {
                    Sync.SafeAsync(RefreshReleasePointersAsync);
                    Sync.SafeAsync(RefreshReleasesAsync);
                });
            }
        }

        #region Release Pointers

        public Task RefreshReleasePointersAsync()
        {
            if (m_RefreshPointersTask == null || m_RefreshPointersTask.IsCompleted
                || (m_RefreshPointersCts?.IsCancellationRequested ?? false))
            {
                m_RefreshPointersCts?.Dispose();
                m_RefreshPointersCts = new CancellationTokenSource();
                m_RefreshPointersTask = RefreshPointersInternal(m_RefreshPointersCts.Token);
            }
            return m_RefreshPointersTask;
        }

        public async Task<IReadOnlyList<ReleasePointerInfo>> FetchReleasePointersForEnvironmentAsync(string environmentId)
        {
            try
            {
                return await m_Fetcher.FetchReleasePointers(environmentId);
            }
            catch (Exception e)
            {
                CoreLogger.LogWarning($"[Releases] Failed to fetch release pointers for env {environmentId}: {e.Message}");
                return null;
            }
        }

        #endregion

        #region Releases

        public void SetActiveRelease(ReleaseInfo releaseInfo)
        {
            m_ReleaseProvider.ActiveRelease = new ReleaseSettings(releaseInfo.Name, releaseInfo.Id);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveReleaseName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveReleaseId)));
        }

        public void ClearActiveRelease()
        {
            m_ReleaseProvider.ActiveRelease = new ReleaseSettings();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveReleaseName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveReleaseId)));
        }

        public Task RefreshReleasesAsync()
        {
            if (m_RefreshReleasesTask == null || m_RefreshReleasesTask.IsCompleted
                || (m_RefreshReleasesCts?.IsCancellationRequested ?? false))
            {
                m_RefreshReleasesCts?.Dispose();
                m_RefreshReleasesCts = new CancellationTokenSource();
                m_RefreshReleasesTask = RefreshReleasesInternal(m_RefreshReleasesCts.Token);
            }
            return m_RefreshReleasesTask;
        }

        #endregion

        public void SetLiveReleasesEnabled(bool enabled)
        {
            m_ReleaseProvider.LiveReleasesEnabled = enabled;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LiveReleasesEnabled)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveReleaseName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveReleaseId)));
        }

        async Task RefreshPointersInternal(CancellationToken ct)
        {
            var activeEnvironmentId = m_EnvironmentService.ActiveEnvironmentId;
            if (activeEnvironmentId == null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReleasePointers)));
                return;
            }

            // Environments haven't been confirmed from the server yet — the cached environment ID
            // may be stale. Skip this request; OnEnvironmentServicePropertyChanged will re-trigger
            // once environments finish loading.
            if (m_EnvironmentService.Environments == null)
                return;

            try
            {
                var result = await m_Fetcher.FetchReleasePointers(activeEnvironmentId.ToString());
                if (ct.IsCancellationRequested)
                    return;

                ReleasePointers = result;
            }
            catch (Unity.Services.Core.Releases.Client.Http.HttpException e)
            {
                if (ct.IsCancellationRequested)
                    return;

                CoreLogger.LogError($"[Releases] Failed to fetch release pointers ({e.Response?.StatusCode}): {e.Message}");
            }
            catch (Exception e)
            {
                if (ct.IsCancellationRequested)
                    return;

                CoreLogger.LogWarning($"[Releases] Failed to fetch release pointers: {e.Message}. Check your network connection and that you are signed into Unity.");
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReleasePointers)));
        }

        async Task RefreshReleasesInternal(CancellationToken ct)
        {
            var activeEnvironmentId = m_EnvironmentService.ActiveEnvironmentId;
            if (activeEnvironmentId == null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Releases)));
                return;
            }

            if (m_EnvironmentService.Environments == null)
                return;

            try
            {
                var result = await m_Fetcher.FetchReleases(activeEnvironmentId.ToString());
                if (ct.IsCancellationRequested)
                    return;

                Releases = result;
            }
            catch (Unity.Services.Core.Releases.Client.Http.HttpException e)
            {
                if (ct.IsCancellationRequested)
                    return;

                CoreLogger.LogError($"[Releases] Failed to fetch releases ({e.Response?.StatusCode}): {e.Message}");
            }
            catch (Exception e)
            {
                if (ct.IsCancellationRequested)
                    return;

                CoreLogger.LogWarning($"[Releases] Failed to fetch releases: {e.Message}. Check your network connection and that you are signed into Unity.");
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Releases)));
        }

        void OnEnvironmentServicePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(IEnvironmentService.ActiveEnvironmentId):
                    m_RefreshPointersCts?.Cancel();
                    m_RefreshReleasesCts?.Cancel();

                    // Switch provider to the new environment's persisted selection rather
                    // than clearing — this lets users switch back and restore their selection.
                    var newEnvId = m_EnvironmentService.ActiveEnvironmentId?.ToString() ?? string.Empty;
                    m_ReleaseProvider.EnvironmentId = newEnvId;

                    ReleasePointers = null;
                    Releases = null;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveReleaseName)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveReleaseId)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReleasePointers)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Releases)));
                    Sync.SafeAsync(RefreshReleasePointersAsync);
                    Sync.SafeAsync(RefreshReleasesAsync);
                    break;
                case nameof(IEnvironmentService.Environments):
                    m_RefreshPointersCts?.Cancel();
                    m_RefreshReleasesCts?.Cancel();
                    ReleasePointers = null;
                    Releases = null;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReleasePointers)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Releases)));
                    Sync.SafeAsync(RefreshReleasePointersAsync);
                    Sync.SafeAsync(RefreshReleasesAsync);
                    break;
            }
        }

        public void Dispose()
        {
            m_EnvironmentService.PropertyChanged -= OnEnvironmentServicePropertyChanged;
            m_RefreshPointersCts?.Cancel();
            m_RefreshPointersCts?.Dispose();
            m_RefreshReleasesCts?.Cancel();
            m_RefreshReleasesCts?.Dispose();
        }
    }
}
