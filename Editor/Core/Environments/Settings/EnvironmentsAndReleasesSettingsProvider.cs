using System;
using System.Collections.Generic;
#if UNITY_6000_7_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
using Unity.Services.Core.Editor.Environments;
using Unity.Services.Core.Editor.Environments.UI;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Services.Core.Editor.Settings
{
    /// <summary>
    /// The class which provides the page in Project Settings for Environments and Releases.
    /// </summary>
#if UNITY_6000_7_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    class EnvironmentsAndReleasesSettingsProvider : EditorGameServiceSettingsProvider
    {
        /// <summary>
        /// Optional title suffix set by the Releases assembly (e.g. " &amp; Live Releases").
        /// </summary>
        internal static string s_TitleSuffix;

        /// <summary>
        /// Optional delegate set by the Releases assembly to inject release pointer UI into the settings page.
        /// </summary>
        internal static Action<EnvironmentSelector, IEnvironmentService> s_ReleaseUiSetup;

        /// <summary>
        /// The <see cref="IEditorGameService"/> associated with this Settings Provider.
        /// </summary>
        protected override IEditorGameService EditorGameService
            => EditorGameServiceRegistry.Instance.GetEditorGameService<EnvironmentIdentifier>();

        /// <summary>
        /// The title.
        /// </summary>
        protected override string Title => EnvironmentsConstants.ServiceName + (s_TitleSuffix ?? "");

        /// <summary>
        /// The description.
        /// </summary>
        protected override string Description { get; } = "Move assets and configurations to backend services from within the editor";

        readonly IEnvironmentService m_EnvironmentService;

        EnvironmentsAndReleasesSettingsProvider(
            IEnvironmentService environmentService,
            string path,
            SettingsScope scopes,
            IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
            m_EnvironmentService = environmentService;
        }

        [SettingsProvider]
        static SettingsProvider CreateEnvironmentsSettingsProvider()
        {
            return new EnvironmentsAndReleasesSettingsProvider(
                EnvironmentServiceRegistry.Instance.GetService<IEnvironmentService>(),
                GenerateProjectSettingsPath(EnvironmentsConstants.ServiceName),
                SettingsScope.Project);
        }

        /// <summary>
        /// The generator for the UI.
        /// </summary>
        /// <returns>The visual element added to the Settings Project page.</returns>
        protected override VisualElement GenerateServiceDetailUI()
        {
            var environmentSelector = new EnvironmentSelector(m_EnvironmentService);
            s_ReleaseUiSetup?.Invoke(environmentSelector, m_EnvironmentService);
            return environmentSelector;
        }

        /// <summary>
        /// The generator for the UI when unsupported.
        /// </summary>
        /// <returns>The visual element added to the Settings Project page when unsupported.</returns>
        protected override VisualElement GenerateUnsupportedDetailUI()
        {
            return GenerateServiceDetailUI();
        }
    }
}
