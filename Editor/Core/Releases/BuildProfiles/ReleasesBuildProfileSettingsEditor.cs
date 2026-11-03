#if UNITY_6000_4_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Core.Editor.Environments;
using Unity.Services.Core.Editor.Releases;
using Unity.Services.Core.Editor.Shared.EditorUtils;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Services.Core.Editor.Releases
{
    [CustomEditor(typeof(ReleasesBuildProfileSettings))]
    internal class ReleasesBuildProfileSettingsEditor : UnityEditor.Editor
    {
        const string k_NonePointerLabel = "None";

        // Pointers fetched for the build profile's currently selected environment.
        // Reset to null each time CreateInspectorGUI is called (fresh inspector).
        IReadOnlyList<ReleasePointerInfo> m_FetchedPointers;

        // Incremented each time a new pointer fetch is triggered.
        // Callbacks from earlier (stale) fetches are ignored when the generation has advanced.
        int m_FetchGeneration;

        public override VisualElement CreateInspectorGUI()
        {
            m_FetchedPointers = null;

            var settings = (ReleasesBuildProfileSettings)target;
            var environmentService = EnvironmentServiceRegistry.Instance.GetService<IEnvironmentService>();
            var releasePointerService = ReleaseServiceRegistry.Instance.GetService<IReleasesService>();

            var root = new VisualElement();

            var envDropdown = new DropdownField("Environment")
            {
                name = "EnvironmentDropdownControl"
            };
            var envRefreshButton = new Button { text = "Refresh", name = "RefreshEnvironmentDropdownButton" };

            var rpDropdown = new DropdownField("Release Pointer")
            {
                name = "ReleasePointerDropdownControl"
            };
            var rpRefreshButton = new Button { text = "Refresh", name = "RefreshReleasePointerDropdownButton" };

            var rpInfoBox = new HelpBox { name = "ReleasePointerInfoBox" };
            rpInfoBox.style.display = DisplayStyle.None;

            root.Add(CreateRow(envDropdown, envRefreshButton));
            root.Add(CreateRow(rpDropdown, rpRefreshButton));
            root.Add(rpInfoBox);

            PopulateEnvironmentDropdown(envDropdown, settings, environmentService);
            PopulateReleasePointerDropdown(rpDropdown, rpInfoBox, settings, m_FetchedPointers);

            envDropdown.RegisterValueChangedCallback(evt =>
            {
                if (settings.selectedEnvironmentName == evt.newValue) return;
                settings.selectedEnvironmentName = evt.newValue;
                settings.selectedReleasePointerName = null; // clear stale pointer for new env
                EditorUtility.SetDirty(settings);
                serializedObject.ApplyModifiedProperties();
                // No SetActiveEnvironment — the build profile must not mutate global editor state.
                TriggerPointerFetchForEnv(rpRefreshButton, rpDropdown, rpInfoBox, settings,
                    environmentService, releasePointerService, evt.newValue);
            });

            envRefreshButton.clicked += () =>
                TriggerEnvironmentRefresh(envRefreshButton, rpRefreshButton, envDropdown, rpDropdown,
                    rpInfoBox, settings, environmentService, releasePointerService);

            rpDropdown.RegisterValueChangedCallback(evt =>
            {
                settings.selectedReleasePointerName = evt.newValue == k_NonePointerLabel ? null : evt.newValue;
                EditorUtility.SetDirty(settings);
                serializedObject.ApplyModifiedProperties();
                UpdateReleasePointerInfoBox(rpInfoBox, evt.newValue, m_FetchedPointers);
            });

            rpRefreshButton.clicked += () =>
                TriggerPointerFetchForEnv(rpRefreshButton, rpDropdown, rpInfoBox, settings,
                    environmentService, releasePointerService, settings.selectedEnvironmentName);

            // Initial fetch: if environments aren't loaded, fetch them (then cascade to pointer fetch).
            // If environments are loaded, fetch pointers for the profile's selected env directly.
            if (environmentService.Environments == null)
            {
                TriggerEnvironmentRefresh(envRefreshButton, rpRefreshButton, envDropdown, rpDropdown,
                    rpInfoBox, settings, environmentService, releasePointerService);
            }
            else
            {
                TriggerPointerFetchForEnv(rpRefreshButton, rpDropdown, rpInfoBox, settings,
                    environmentService, releasePointerService, settings.selectedEnvironmentName);
            }

            return root;
        }

        static VisualElement CreateRow(DropdownField dropdown, Button button)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            dropdown.style.flexGrow = 1;
            dropdown.style.flexShrink = 0;

            button.style.flexGrow = 0;
            button.style.flexShrink = 1;
            button.style.marginLeft = 5;

            row.Add(dropdown);
            row.Add(button);
            return row;
        }

        void TriggerEnvironmentRefresh(Button envRefreshButton, Button rpRefreshButton,
            DropdownField envDropdown, DropdownField rpDropdown, HelpBox rpInfoBox,
            ReleasesBuildProfileSettings settings, IEnvironmentService environmentService,
            IReleasesService releasePointerService)
        {
            envRefreshButton.SetEnabled(false);
            rpRefreshButton.SetEnabled(false);
            rpDropdown.SetEnabled(false);
            Sync.SafeAsync(
                () => environmentService.RefreshEnvironmentsAsync(),
                _ =>
                {
                    PopulateEnvironmentDropdown(envDropdown, settings, environmentService);
                    envRefreshButton.SetEnabled(true);
                    TriggerPointerFetchForEnv(rpRefreshButton, rpDropdown, rpInfoBox, settings,
                        environmentService, releasePointerService, settings.selectedEnvironmentName);
                });
        }

        void TriggerPointerFetchForEnv(Button button, DropdownField dropdown, HelpBox infoBox,
            ReleasesBuildProfileSettings settings, IEnvironmentService envService,
            IReleasesService releasesService, string envName)
        {
            button.SetEnabled(false);
            dropdown.SetEnabled(false);

            var envInfo = envService.EnvironmentInfoFromName(envName);
            if (!envInfo.HasValue)
            {
                m_FetchedPointers = null;
                PopulateReleasePointerDropdown(dropdown, infoBox, settings, m_FetchedPointers);
                button.SetEnabled(true);
                dropdown.SetEnabled(true);
                return;
            }

            var generation = ++m_FetchGeneration;
            IReadOnlyList<ReleasePointerInfo> fetchedResult = null;
            Sync.SafeAsync(
                async () => { fetchedResult = await releasesService.FetchReleasePointersForEnvironmentAsync(envInfo.Value.Id.ToString()); },
                _ =>
                {
                    if (generation != m_FetchGeneration) return; // stale — a newer fetch superseded this one
                    m_FetchedPointers = fetchedResult;
                    PopulateReleasePointerDropdown(dropdown, infoBox, settings, m_FetchedPointers);
                    button.SetEnabled(true);
                    dropdown.SetEnabled(true);
                });
        }

        static void PopulateEnvironmentDropdown(DropdownField dropdown, ReleasesBuildProfileSettings settings,
            IEnvironmentService environmentService)
        {
            if (settings == null) return;
            var choices = new List<string>();
            if (environmentService.Environments != null) {
                foreach (var env in environmentService.Environments) {
                    choices.Add(env.Name);
                }
            }

            dropdown.choices = choices;
            var savedName = settings.selectedEnvironmentName;
            if (choices.Contains(savedName))
            {
                dropdown.SetValueWithoutNotify(savedName);
            }
            else if (choices.Count > 0)
            {
                settings.selectedEnvironmentName = choices[0];
                settings.selectedReleasePointerName = null; // clear stale pointer — env changed without callback
                EditorUtility.SetDirty(settings);
                dropdown.SetValueWithoutNotify(choices[0]);
            }
            else
            {
                dropdown.SetValueWithoutNotify(string.Empty);
            }
        }

        static void PopulateReleasePointerDropdown(DropdownField dropdown, HelpBox infoBox,
            ReleasesBuildProfileSettings settings, IReadOnlyList<ReleasePointerInfo> pointers)
        {
            if (settings == null) return;
            var choices = new List<string> { k_NonePointerLabel };
            if (pointers != null) {
                foreach (var pointer in pointers) {
                    choices.Add(pointer.Name);
                }
            }

            dropdown.choices = choices;
            var savedName = settings.selectedReleasePointerName;
            var display = !string.IsNullOrEmpty(savedName) && choices.Contains(savedName)
                ? savedName
                : k_NonePointerLabel;
            // If we have a real (non-null) pointer list and the saved pointer isn't in it,
            // the pointer no longer exists for this environment — clear it so builds don't
            // bake a stale pointer name. Skip this when pointers is null (not yet fetched).
            if (pointers != null && display == k_NonePointerLabel && !string.IsNullOrEmpty(savedName))
            {
                settings.selectedReleasePointerName = null;
                EditorUtility.SetDirty(settings);
            }
            dropdown.SetValueWithoutNotify(display);
            UpdateReleasePointerInfoBox(infoBox, display, pointers);
        }

        static void UpdateReleasePointerInfoBox(HelpBox infoBox, string pointerName,
            IReadOnlyList<ReleasePointerInfo> pointers)
        {
            if (string.IsNullOrEmpty(pointerName) || pointerName == k_NonePointerLabel)
            {
                infoBox.style.display = DisplayStyle.None;
                return;
            }

            var pointerInfo = pointers?
                .Where(p => p.Name == pointerName)
                .Cast<ReleasePointerInfo?>()
                .FirstOrDefault();

            if (pointerInfo == null)
            {
                infoBox.style.display = DisplayStyle.None;
                return;
            }

            infoBox.style.display = DisplayStyle.Flex;
            var hasRelease = !string.IsNullOrEmpty(pointerInfo.Value.ReleaseName);
            if (hasRelease)
            {
                infoBox.messageType = HelpBoxMessageType.Info;
                infoBox.text = $"Currently pointing at release: {pointerInfo.Value.ReleaseName}";
            }
            else
            {
                infoBox.messageType = HelpBoxMessageType.Warning;
                infoBox.text = "This pointer is not currently pointing at any release. Use 'Manage release pointers' in the Environment Selector settings to assign one.";
            }
        }
    }
}
#endif
