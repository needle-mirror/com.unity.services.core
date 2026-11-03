using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core.Editor.Environments;
using Unity.Services.Core.Editor.Releases;
using Unity.Services.Core.Editor.Shared.EditorUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Services.Core.Editor.Releases.UI
{
    class ReleaseSelector
    {
        const string k_UxmlPathReleaseDropdown = "Packages/com.unity.services.core/Editor/Core/Releases/UI/Assets/ReleaseDropdown.uxml";

        readonly IReleasesService m_ReleaseService;
        readonly IEnvironmentService m_EnvironmentService;
        readonly IReleaseDashboardUrlResolver m_DashboardUrlResolver;
        readonly VisualElement m_Container;

        VisualElement m_ReleaseContainerDropdownSection;
        VisualElement m_ReleaseContainerFetching;
        VisualElement m_ReleaseContainerWarning;

        Button m_RefreshReleaseButton;
        Toggle m_UseLiveReleasesToggle;
        DropdownField m_ReleaseDropdownControl;

        public ReleaseSelector(
            VisualElement container,
            IReleasesService releaseService,
            IEnvironmentService environmentService,
            IReleaseDashboardUrlResolver dashboardUrlResolver = null)
        {
            m_Container = container;
            m_ReleaseService = releaseService;
            m_EnvironmentService = environmentService;
            m_DashboardUrlResolver = dashboardUrlResolver ?? new ReleaseDashboardUrlResolver();

            // EnvironmentSelector hides this section unconditionally; we show it now that we're managing it.
            container.Q(UxmlNames.ReleasesSection).style.display = DisplayStyle.Flex;

            SetupReleaseDropdown(container);
            SetupReleaseWarning(container);
            SetupManageReleases(container);

            m_ReleaseService.PropertyChanged += OnReleaseServicePropertyChanged;
            m_EnvironmentService.PropertyChanged += OnEnvironmentServicePropertyChanged;
        }

        void OnReleaseServicePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(IReleasesService.Releases):
                    OnReleasesRefreshed();
                    break;
                case nameof(IReleasesService.ActiveReleaseName):
                case nameof(IReleasesService.ActiveReleaseId):
                    SyncDropdownToActiveRelease();
                    break;
            }
        }

        void OnEnvironmentServicePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(IEnvironmentService.ActiveEnvironmentId))
                return;

            var manageReleasesContainer = m_Container.Q(UxmlNames.ManageReleasesContainer);
            if (manageReleasesContainer == null)
                return;

#if ENABLE_EDITOR_GAME_SERVICES
            manageReleasesContainer.style.display = m_EnvironmentService.ActiveEnvironmentId.HasValue
                ? DisplayStyle.Flex
                : DisplayStyle.None;
#else
            manageReleasesContainer.style.display = DisplayStyle.None;
#endif
        }

        void SetupReleaseDropdown(VisualElement containerElement)
        {
            m_ReleaseContainerDropdownSection = containerElement.Q(UxmlNames.ReleaseDropdownSection);
            m_ReleaseContainerFetching = containerElement.Q(UxmlNames.FetchingReleasesSection);

            m_ReleaseContainerDropdownSection.style.display = DisplayStyle.None;

            var uxmlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_UxmlPathReleaseDropdown);
            if (uxmlAsset != null)
            {
                uxmlAsset.CloneTree(m_ReleaseContainerDropdownSection);
                m_ReleaseDropdownControl = containerElement.Q<DropdownField>(UxmlNames.ReleaseDropdownControl);
                m_ReleaseDropdownControl.RegisterValueChangedCallback(OnDropdownReleaseChanged);
                var refreshButton = containerElement.Q<Button>(UxmlNames.RefreshReleaseButton);
                SetupRefreshButton(refreshButton);
            }
            else
            {
                throw new MissingReferenceException("Could not find a uxml asset to load.");
            }

            m_UseLiveReleasesToggle = containerElement.Q<Toggle>(UxmlNames.UseLiveReleasesToggle);
            m_UseLiveReleasesToggle.SetValueWithoutNotify(m_ReleaseService.LiveReleasesEnabled);
            m_UseLiveReleasesToggle.RegisterValueChangedCallback(OnUseLiveReleasesToggleChanged);
            UpdateDropdownEnabled(m_UseLiveReleasesToggle.value);
        }

        void SetupManageReleases(VisualElement containerElement)
        {
            var container = containerElement.Q(UxmlNames.ManageReleasesContainer);
#if ENABLE_EDITOR_GAME_SERVICES
            container.style.display = m_EnvironmentService.ActiveEnvironmentId.HasValue
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            var clickable = new Clickable(() =>
            {
                if (m_EnvironmentService.ActiveEnvironmentId.HasValue)
                {
                    Application.OpenURL(
                        m_DashboardUrlResolver.ManageReleases(m_EnvironmentService.ActiveEnvironmentId.Value));
                }
            });
            container.AddManipulator(clickable);
#else
            container.style.display = DisplayStyle.None;
#endif
        }

        void SetupReleaseWarning(VisualElement containerElement)
        {
            m_ReleaseContainerWarning = containerElement.Q(UxmlNames.DefaultReleaseSection);
            m_ReleaseContainerWarning.style.display = DisplayStyle.None;
        }

        void SetupRefreshButton(Button button)
        {
            if (button == null)
                return;

            m_RefreshReleaseButton = button;
            m_RefreshReleaseButton.clicked += () =>
            {
                m_RefreshReleaseButton.AddToClassList(UxmlClasses.UnityDisabled);
                Sync.SafeAsync(m_ReleaseService.RefreshReleasesAsync, OnDoneRefreshing);
            };
        }

        void OnDoneRefreshing(Task refreshTask)
        {
            m_RefreshReleaseButton?.RemoveFromClassList(UxmlClasses.UnityDisabled);
        }

        void OnReleasesRefreshed()
        {
            if (m_ReleaseService.Releases == null)
            {
                SetVisibleContainer(m_ReleaseContainerFetching);
                return;
            }

            var choices = m_ReleaseService.Releases.Select(r => r.Name).ToList();
            choices.Insert(0, ReleasesConstants.NoReleasesAvailableLabel);
            m_ReleaseDropdownControl.choices = choices;

            var activeName = m_ReleaseService.ActiveReleaseName;
            if (!string.IsNullOrEmpty(activeName) && !m_ReleaseService.Releases.Any(r => r.Name == activeName))
            {
                m_ReleaseService.ClearActiveRelease();
            }

            SyncDropdownToActiveRelease();
            SetVisibleContainer(m_ReleaseContainerDropdownSection);
        }

        void SyncDropdownToActiveRelease()
        {
            var activeName = m_ReleaseService.ActiveReleaseName;
            m_ReleaseDropdownControl.SetValueWithoutNotify(
                string.IsNullOrEmpty(activeName) ? ReleasesConstants.NoReleasesAvailableLabel : activeName);

            var isNoneSelected = m_ReleaseDropdownControl.value == ReleasesConstants.NoReleasesAvailableLabel;
            m_ReleaseContainerWarning.style.display = isNoneSelected ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnDropdownReleaseChanged(ChangeEvent<string> changeEvent)
        {
            if (changeEvent.newValue == ReleasesConstants.NoReleasesAvailableLabel)
            {
                m_ReleaseService.ClearActiveRelease();
                return;
            }

            var info = m_ReleaseService.Releases?
                .Cast<ReleaseInfo?>()
                .FirstOrDefault(r => r.Value.Name == changeEvent.newValue);

            if (info.HasValue)
            {
                m_ReleaseService.SetActiveRelease(info.Value);
            }
        }

        void OnUseLiveReleasesToggleChanged(ChangeEvent<bool> changeEvent)
        {
            m_ReleaseService.SetLiveReleasesEnabled(changeEvent.newValue);
            UpdateDropdownEnabled(changeEvent.newValue);
        }

        void UpdateDropdownEnabled(bool enabled)
        {
            m_ReleaseContainerDropdownSection.SetEnabled(enabled);
        }

        void SetVisibleContainer(VisualElement containerElement)
        {
            m_ReleaseContainerDropdownSection.style.display =
                containerElement == m_ReleaseContainerDropdownSection ? DisplayStyle.Flex : DisplayStyle.None;
            m_ReleaseContainerFetching.style.display =
                containerElement == m_ReleaseContainerFetching ? DisplayStyle.Flex : DisplayStyle.None;
            UpdateDropdownEnabled(m_UseLiveReleasesToggle.value);
        }

        static class UxmlNames
        {
            public const string UseLiveReleasesToggle = "UseLiveReleasesToggle";
            public const string ReleasesSection = "Releases Section";
            public const string ReleaseDropdownSection = "Release Dropdown Section";
            public const string FetchingReleasesSection = "Fetching Releases Section";
            public const string ManageReleasesContainer = "Manage Releases Container";
            public const string DefaultReleaseSection = "Default Release Section";
            public const string ReleaseDropdownControl = "ReleaseDropdownControl";
            public const string RefreshReleaseButton = "RefreshReleaseDropdownButton";
        }

        static class UxmlClasses
        {
            public const string UnityDisabled = "unity-disabled";
        }
    }
}
