using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.Services.Core.Editor.Environments.Analytics;
using Unity.Services.Core.Editor.Settings;
using Unity.Services.Core.Editor.Shared.EditorUtils;
using Unity.Services.Core.Internal;
using UnityEditor;

namespace Unity.Services.Core.Editor.Environments
{
    sealed class EnvironmentService : IEnvironmentService, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public IReadOnlyCollection<EnvironmentInfo> Environments { get; private set; }

        public Guid? ActiveEnvironmentId
        {
            get
            {
                var activeEnv = m_EnvironmentProvider.ActiveEnvironment;
                if (activeEnv.EnvironmentId != Guid.Empty)
                {
                    return activeEnv.EnvironmentId;
                }

                return null;
            }
        }

        const string ProductionEnvironmentName = "production";

        readonly IEnvironmentFetcher m_EnvironmentFetcher;
        readonly IEnvironmentProvider m_EnvironmentProvider;
        readonly IEnvironmentAnalytics m_EnvironmentAnalytics;
        readonly EditorValueTracker<string> m_ProjectIdTracker;

        Task m_RefreshTask;

        public EnvironmentService(
            IEnvironmentFetcher environmentFetcher,
            IEnvironmentProvider environmentProvider,
            IEnvironmentAnalytics environmentAnalytics)
        {
            m_EnvironmentFetcher = environmentFetcher;
            m_EnvironmentProvider = environmentProvider;
            m_EnvironmentAnalytics = environmentAnalytics;

            m_ProjectIdTracker = new EditorValueTracker<string>(() => CloudProjectSettings.projectId);
            m_ProjectIdTracker.ValueChanged += (_, __) =>
            {
                if (string.IsNullOrEmpty(CloudProjectSettings.projectId))
                {
                    return;
                }

                RefreshEnvironmentsAsync();
            };

            m_EnvironmentProvider.PropertyChanged += OnEnvironmentProviderPropertyChanged;
        }

        public Task RefreshEnvironmentsAsync()
        {
            if (m_RefreshTask == null || m_RefreshTask.IsCompleted)
            {
                m_RefreshTask = RefreshEnvironmentsInternal();
            }

            return m_RefreshTask;
        }

        public async Task<EnvironmentInfo> CreateEnvironmentAsync(string name)
        {
            var created = await m_EnvironmentFetcher.CreateEnvironment(name);
            if (!created.HasValue)
            {
                throw new InvalidOperationException(
                    "Failed to create environment. Ensure you are signed in and your project is linked.");
            }

            await RefreshEnvironmentsAsync();
            return created.Value;
        }

        public async Task DeleteEnvironmentAsync(string name)
        {
            if (string.Equals(name, ProductionEnvironmentName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"The '{ProductionEnvironmentName}' environment cannot be deleted.");
            }

            await RefreshEnvironmentsAsync();

            var environmentInfo = this.EnvironmentInfoFromName(name);
            if (!environmentInfo.HasValue)
            {
                throw new EnvironmentNotFoundException($"Could not find environment with name '{name}'.");
            }

            var deleted = await m_EnvironmentFetcher.DeleteEnvironment(environmentInfo.Value.Id.ToString());
            if (!deleted)
            {
                throw new InvalidOperationException(
                    "Failed to delete environment. Ensure you are signed in and your project is linked.");
            }

            await RefreshEnvironmentsAsync();
        }

        #region Environments

        public void SetActiveEnvironment(EnvironmentInfo environmentInfo)
        {
            SetActiveEnvironmentInternal(environmentInfo);
        }

        public void SetActiveEnvironment(string environmentName)
        {
            var environmentInfo = this.EnvironmentInfoFromName(environmentName);
            if (environmentInfo.HasValue)
            {
                SetActiveEnvironmentInternal(environmentInfo.Value);
            }
            else
            {
                throw new EnvironmentNotFoundException($"Could not find environment with name '{environmentName}'.");
            }
        }

        public void SetActiveEnvironment(Guid environmentGuid)
        {
            var environmentInfo = this.EnvironmentInfoFromId(environmentGuid);
            if (environmentInfo.HasValue)
            {
                SetActiveEnvironmentInternal(environmentInfo.Value);
            }
            else
            {
                throw new EnvironmentNotFoundException($"Could not find environment with guid '{environmentGuid}'.");
            }
        }

        // Returns the cleared environment name if the active environment was invalid and cleared; null otherwise.
        string ClearActiveEnvironmentIfInvalid()
        {
            var activeId = ActiveEnvironmentId;
            if (!activeId.HasValue || Environments == null || this.EnvironmentInfoFromId(activeId) != null)
                return null;

            var clearedName = m_EnvironmentProvider.ActiveEnvironment.EnvironmentName;
            m_EnvironmentProvider.ActiveEnvironment = new EnvironmentSettings();
            return clearedName;
        }

        void SetActiveEnvironmentInternal(EnvironmentInfo environment, bool isDefault = false)
        {
            m_EnvironmentProvider.ActiveEnvironment = new EnvironmentSettings(environment.Name, environment.Id);
            if (!isDefault)
            {
                m_EnvironmentAnalytics.SendEnvironmentChangedEvent(ActiveEnvironmentId.ToString());
            }
        }

        #endregion

        async Task RefreshEnvironmentsInternal()
        {
            Environments = null;
            Environments = await m_EnvironmentFetcher.FetchEnvironments();

            var clearedName = ClearActiveEnvironmentIfInvalid();

            if (!ActiveEnvironmentId.HasValue)
            {
                var production = this.EnvironmentInfoFromName(ProductionEnvironmentName);
                if (production.HasValue)
                {
                    var message = string.IsNullOrEmpty(clearedName)
                        ? $"[Environments] No active environment set; defaulting to '{ProductionEnvironmentName}'. To use a different environment, select one in Project Settings > Services."
                        : $"[Environments] Saved environment '{clearedName}' no longer exists - defaulting to '{ProductionEnvironmentName}'. To use a different environment, select one in Project Settings > Services.";
                    CoreLogger.LogWarning(message);
                    SetActiveEnvironmentInternal(production.Value, isDefault: true);
                }
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Environments)));
        }

        void OnEnvironmentProviderPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(IEnvironmentProvider.ActiveEnvironment))
            {
                var clearedName = ClearActiveEnvironmentIfInvalid();
                if (clearedName == null)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveEnvironmentId)));
                }
                else
                {
                    CoreLogger.LogWarning(
                        $"[Environments] Saved environment '{clearedName}' no longer exists in the current project. Clearing active environment. Select a valid environment in Project Settings > Services.");
                }
            }
        }

        public void Dispose()
        {
            m_EnvironmentProvider.PropertyChanged -= OnEnvironmentProviderPropertyChanged;
            m_ProjectIdTracker.Dispose();

            if (m_RefreshTask != null && m_RefreshTask.IsCompleted)
            {
                m_RefreshTask.Dispose();
            }
        }
    }
}
