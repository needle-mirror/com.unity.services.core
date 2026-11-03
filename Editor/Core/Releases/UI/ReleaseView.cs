using Unity.Services.Core.Editor.Environments;
using Unity.Services.Core.Editor.Releases;
using Unity.Services.Core.Editor.Shared.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Services.Core.Editor.Releases.UI
{
    /// <summary>
    /// The <see cref="VisualElement"/> that displays the currently active release
    /// </summary>
#if UNITY_2023_3_OR_NEWER
    [UxmlElement]
#endif
    public partial class ReleaseView : VisualElement
    {
        const string k_TemplatePath = "Packages/com.unity.services.core/Editor/Core/Releases/UI/Assets/ReleaseView.uxml";
        const string k_StylePath = "Packages/com.unity.services.core/Editor/Core/Releases/UI/Assets/ReleaseView_style.uss";

        static readonly string k_ReleaseSettingsMenuItemText = L10n.Tr("Release and Environment Settings");
        static readonly string k_NoRelease = L10n.Tr("No release");
        static readonly string k_Loading = L10n.Tr("Loading...");

        internal const string LoadingClass = "loading";

        readonly ModelBinding<IReleasesApi> m_ReleasesBinding;

        /// <summary>
        /// Constructor for <see cref="ReleaseView"/>
        /// </summary>
        public ReleaseView()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_TemplatePath);
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StylePath));
            visualTreeAsset.CloneTree(this);

            var toolbarMenu = this.Q<ToolbarMenu>();
            toolbarMenu.menu.AppendAction(
                k_ReleaseSettingsMenuItemText,
                _ => OpenReleaseSettings());

            m_ReleasesBinding = new ModelBinding<IReleasesApi>(this);
            m_ReleasesBinding.BindProperty(nameof(IReleasesApi.ActiveReleaseName), UpdateBindings);
            m_ReleasesBinding.BindProperty(nameof(IReleasesApi.Releases), UpdateBindings);
            m_ReleasesBinding.BindProperty(nameof(IReleasesApi.LiveReleasesEnabled), UpdateBindings);
        }

        /// <summary>
        /// Bind the releases service
        /// </summary>
        /// <param name="releasesApi">The service</param>
        public void Bind(IReleasesApi releasesApi)
        {
            m_ReleasesBinding.Source = releasesApi;
        }

        void UpdateBindings(IReleasesApi service)
        {
            var menu = this.Q<ToolbarMenu>();
            var isLoading = service.Releases == null;
            menu.SetEnabled(!isLoading && service.LiveReleasesEnabled);

            if (isLoading)
            {
                AddToClassList(LoadingClass);
                menu.text = k_Loading;
            }
            else
            {
                RemoveFromClassList(LoadingClass);
                var name = service.ActiveReleaseName;
                menu.text = string.IsNullOrEmpty(name) ? k_NoRelease : name;
            }
        }

        internal static EditorWindow OpenReleaseSettings()
        {
            return SettingsService.OpenProjectSettings(EnvironmentsConstants.SettingsLocation);
        }

#if !UNITY_2023_3_OR_NEWER
        new class UxmlFactory : UxmlFactory<ReleaseView> {} // NOSONAR
#endif
    }
}
